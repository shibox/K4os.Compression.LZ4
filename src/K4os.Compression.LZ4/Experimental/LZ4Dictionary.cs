// using System;
// using K4os.Compression.LZ4.Engine;
// using K4os.Compression.LZ4.Internal;
//
// namespace K4os.Compression.LZ4.Experimental
// {
// 	public class LZ4Dictionary: UnmanagedResources
// 	{
// 		private readonly LL.LZ4_stream_t _snapshot;
//
// 		public unsafe LZ4Dictionary(ReadOnlySpan<byte> dictionary)
// 		{
// 			CloneLast64K(dictionary, out var bytes, out var length);
// 			CreateContextSnapshot(ref _snapshot, bytes, length);
// 		}
//
// 		private static unsafe void CloneLast64K(
// 			ReadOnlySpan<byte> bytes, out byte* target, out int length)
// 		{
// 			var source = bytes.Slice(Math.Max(bytes.Length - Mem.K64, 0));
// 			length = source.Length;
// 			target = (byte*)Mem.Alloc(length);
// 			fixed (byte* sourceP = source)
// 				Mem.Copy(target, sourceP, length);
// 		}
// 		
// 		internal unsafe LL.LZ4_stream_t* CreateEncoderContext()
// 		{
// 			var context = LL.LZ4_createStream();
// 			fixed (byte* snapshotP = &_snapshot)
// 				Mem.Copy((byte*) context, snapshotP, sizeof(LL.LZ4_stream_t));
// 			return context;
// 		}
// 		
// 		internal unsafe LL.LZ4_streamDecode_t* CreateDecoderContext()
// 		{
// 			var context = LL.LZ4_createStreamDecode();
// 			LL.LZ4_setStreamDecode(context, _snapshot.dictionary, (int) _snapshot.dictSize);
// 			return context;
// 		}
//
// 		private static unsafe void CreateContextSnapshot(
// 			ref LL.LZ4_stream_t context, byte* bytes, int length)
// 		{
// 			fixed (LL.LZ4_stream_t* contextP = &context)
// 				LLxx.LZ4_loadDict(contextP, bytes, length);
// 		}
//
// 		protected override unsafe void ReleaseUnmanaged()
// 		{
// 			Mem.Free(_snapshot.dictionary);
// 		}
// 	}
// }
