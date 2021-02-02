using System;
using K4os.Compression.LZ4.Internal;

namespace K4os.Compression.LZ4.Encoders
{
	/// <summary>
	/// Base class for LZ4 encoders. Provides basic functionality shared by
	/// <see cref="LZ4BlockEncoder"/>, <see cref="LZ4FastChainEncoder"/>,
	/// and <see cref="LZ4HighChainEncoder"/> encoders. Do not used directly.
	/// </summary>
	public abstract unsafe class LZ4EncoderBase: UnmanagedResources, ILZ4Encoder
	{
		private readonly byte* _inputBuffer;
		private readonly int _inputLength;
		private readonly int _blockSize;

		private int _inputIndex;
		private int _inputPointer;

		/// <summary>Creates new instance of encoder.</summary>
		/// <param name="chaining">Needs to be <c>true</c> if using dependent blocks.</param>
		/// <param name="blockSize">Block size.</param>
		/// <param name="extraBlocks">Number of extra blocks.</param>
		/// <param name="dictionary">External dictionary (optional, will be copied).</param>
		/// <param name="dictionaryLength">External dictionary length (if exists).</param>
		protected LZ4EncoderBase(
			bool chaining, int blockSize, int extraBlocks,
			byte* dictionary, int dictionaryLength)
		{
			blockSize = Mem.RoundUp(Math.Max(blockSize, Mem.K1), Mem.K1);
			extraBlocks = Math.Max(extraBlocks, 0);
			var dictSize = chaining ? Mem.K64 : 0;
			
			_blockSize = blockSize;
			_inputLength = dictSize + (1 + extraBlocks) * blockSize + 32;
			_inputIndex = _inputPointer = 0;
			_inputBuffer = (byte*) Mem.Alloc(_inputLength + 8);
			
			PreloadDict(dictionary, Math.Min(dictSize, dictionaryLength));
		}

		/// <inheritdoc />
		public int BlockSize => _blockSize;

		/// <inheritdoc />
		public int BytesReady => _inputPointer - _inputIndex;
		
		private void PreloadDict(byte* dictionary, int dictionaryLength)
		{
			if (dictionary == null || dictionaryLength <= 0)
				return;

			if (dictionaryLength > Mem.K64)
			{
				dictionary += dictionaryLength - Mem.K64;
				dictionaryLength = Mem.K64;
			}

			Mem.Move(_inputBuffer, dictionary, dictionaryLength);
			_inputIndex = _inputPointer = dictionaryLength;
		}
		
		/// <summary>
		/// Initialized encoder. Please note: this method can and must be called from constructor.
		/// As it depends/may depend on virtual methods it cannot be called from base class
		/// constructor, so it relies on discipline of implementor of derived class. 
		/// </summary>
		protected void Initialize()
		{
			var dictionary = _inputBuffer;
			var dictionaryLength = _inputIndex;
			if (dictionaryLength > 0) LoadDict(dictionary, dictionaryLength);
		}

		/// <inheritdoc />
		public int Topup(byte* source, int length)
		{
			ThrowIfDisposed();

			if (length == 0)
				return 0;

			var spaceLeft = _inputIndex + _blockSize - _inputPointer;
			if (spaceLeft <= 0)
				return 0;

			var chunk = Math.Min(spaceLeft, length);
			Mem.Move(_inputBuffer + _inputPointer, source, chunk);
			_inputPointer += chunk;

			return chunk;
		}

		/// <inheritdoc />
		public int Encode(byte* target, int length, bool allowCopy)
		{
			ThrowIfDisposed();

			var sourceLength = _inputPointer - _inputIndex;
			if (sourceLength <= 0)
				return 0;

			var encoded = EncodeBlock(_inputBuffer + _inputIndex, sourceLength, target, length);

			if (encoded <= 0)
				throw new InvalidOperationException(
					"Failed to encode chunk. Target buffer too small.");

			if (allowCopy && encoded >= sourceLength)
			{
				Mem.Move(target, _inputBuffer + _inputIndex, sourceLength);
				encoded = -sourceLength;
			}

			Commit();

			return encoded;
		}
		
		private void Commit()
		{
			_inputIndex = _inputPointer;
			if (_inputIndex + _blockSize <= _inputLength)
				return;

			_inputIndex = _inputPointer = CopyDict(_inputBuffer, _inputPointer);
		}
		
		/// <summary>Encodes single block using appropriate algorithm.</summary>
		/// <param name="source">Source buffer.</param>
		/// <param name="sourceLength">Source buffer length.</param>
		/// <param name="target">Target buffer.</param>
		/// <param name="targetLength">Target buffer length.</param>
		/// <returns>Number of bytes actually written to target buffer.</returns>
		protected abstract int EncodeBlock(
			byte* source, int sourceLength, byte* target, int targetLength);

		/// <summary>Copies current dictionary.</summary>
		/// <param name="target">Target buffer.</param>
		/// <param name="dictionaryLength">Dictionary length.</param>
		/// <returns>Dictionary length.</returns>
		protected abstract int CopyDict(byte* target, int dictionaryLength);

		/// <summary>
		/// Preloads dictionary. The dictionary is already in place, all is needed is to call
		/// adequate LZ4 method. 
		/// </summary>
		/// <param name="dictionary">Dictionary start.</param>
		/// <param name="dictionaryLength">Dictionary length.</param>
		protected abstract void LoadDict(byte* dictionary, int dictionaryLength);

		/// <inheritdoc />
		protected override void ReleaseUnmanaged()
		{
			base.ReleaseUnmanaged();
			Mem.Free(_inputBuffer);
		}
	}
}
