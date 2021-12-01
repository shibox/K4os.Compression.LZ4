//using System;
//using System.Diagnostics;
//using TestHelpers;
//using Xunit;
//
//namespace K4os.Compression.LZ4.Test
//{
//	public class PartialDecoding
//	{
//		[Theory]
//		[InlineData(1000)]
//		public void DecodingIntoExactLengthBuffer(int length)
//		{
//			var original = new byte[length];
//			Lorem.Fill(original, 0, original.Length);
//			var encoded = new byte[LZ4Codec.MaximumOutputSize(original.Length)];
//			var encodedLength = LZ4Codec.Encode(
//				original, 0, original.Length, 
//				encoded, 0, encoded.Length);
//			
//			Assert.True(encodedLength > 0);
//
//			var decoded = new byte[original.Length];
//			var decodedLength = LZ4Codec.Decode(
//				encoded, 0, encodedLength, 
//				decoded, 0, decoded.Length);
//			
//			Assert.Equal(length, decodedLength);
//			
//			Tools.SameBytes(original, decoded);			
//		}
//
//		[Theory]
//		[InlineData(5, 256)]
//		[InlineData(12, 256)]
//		[InlineData(128, 256)]
//		[InlineData(455, 1024)]
//		[InlineData(1024*1024, 256)]
//		public void BlockCanBePartiallyDecoded(int prefix, int margin)
//		{
//			var original = new byte[1024*1024];
//			Lorem.Fill(original, 0, original.Length);
//			var encoded = new byte[LZ4Codec.MaximumOutputSize(original.Length)];
//			var encodedLength = LZ4Codec.Encode(
//				original, 0, original.Length, 
//				encoded, 0, encoded.Length);
//			
//			Assert.True(encodedLength > 0);
//
//			var decoded = new byte[Math.Min(prefix + margin, original.Length)];
//			_ = LZ4Codec.Decode(
//				encoded, 0, encodedLength, 
//				decoded, 0, decoded.Length);
//			
//			var overlapLength = Math.Min(prefix, original.Length);
//			
//			Tools.SameBytes(
//				original.AsSpan(0, overlapLength), 
//				decoded.AsSpan(0, overlapLength));			
//		}
//	}
//}
