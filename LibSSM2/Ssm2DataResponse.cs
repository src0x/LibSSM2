// Ssm2DataResponse.cs: Abstract class providing common functionality
//                      for data reponse packet types.

/* Copyright (C) 2010 src0x
 *
 * This file is part of LibSSM2.
 *
 * LibSSM2 is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * LibSSM2 is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with LibSSM2.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Subaru.SSM2
{
	// TODO Find attribute to hide this abstract class (code completion).
	// only valid on properties etc.
	// [DebuggerBrowsable(DebuggerBrowsableState.Never)]

	/// <summary>
	/// Abstract base class to provide the very similar functionality
	/// for variable data reponse packet types.
	/// Should not be used by consumer code.
	/// Used by read-addresses-A8 and read-block-A0 response packets
	/// as these packets differ only by command byte.
	/// Packet consists of: 5 header + X data + 1 checksum bytes.
	/// </summary>
	[DebuggerNonUserCode]
	public abstract class Ssm2DataResponse : Ssm2Packet
	{
		// do require at least one content byte, min is 7 bytes
		private const int PacketSizeSpecificMin = HeaderLength + 1 + 1;

		#region constructors

		protected Ssm2DataResponse ()
		{
			this.count = PacketSizeSpecificMin;
		}

		protected Ssm2DataResponse (byte[] buffer)
			: base (buffer, PacketSizeSpecificMin)
		{
		}

		/// <summary>
		/// Create a new packet.
		/// Uses optimal buffer size. Packet is not finished yet!
		/// </summary>
		protected Ssm2DataResponse (Ssm2Device destination,
		                            Ssm2Device source,
		                            IList<byte> data)
			: base (HeaderLength + 1 + data.Count)
		{
			// needed buffer size: 5 byte header + x * data + 1 checksum

			this.Destination = destination;
			this.Source = source;
			this.Data = data;
			// Data setter has already set Count.
		}

		#endregion constructors

		// Implementation notes:
		// Returning IList<int> instead of IEnumerable<int> because of performance benefits
		// for following analysis (e.g. LINQ queries).

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		/// <value>
		/// The data.
		/// </value>
		public IList<byte> Data {
			get {
				int adrCount = DataCount;
				var data = new List<byte> (adrCount);
				// optimize using local variable
				byte[] buffer = this.buffer;
				// set index to 1st addresses byte = HeaderLength, there is no padding byte
				int i = HeaderLength;
				for (int adrNr = 0; adrNr < adrCount; adrNr++) {
					data.Add (buffer[i++]);
				}
				return data;
			}
			set {
				// optimize using local variable
				int i = HeaderLength;
				// foreach well optimized by compiler when using an array
				foreach (int d in value) {
					buffer[i++] = (byte)d;
				}
				UpdateFlags (SetProperties.DataAll);

				// total packet size known here, include checksum
				this.count = i + 1;
			}
		}

		/// <summary>
		/// Calculates the number of addresses based on packet length.
		/// </summary>
		/// <returns>0 if there is an error.</returns>
		public int DataCount {
			get {
				// packet should consist of: 5 byte header + x * data bytes + 1 checksum
				int dataCount = this.count - (HeaderLength + 1);
				// grouping constants together optimizes IL code:
				//int dataCount = this.count - (HeaderLength + 1);
				if (dataCount >= 1 && dataCount <= (PacketSizeMax - HeaderLength - 1))
					return dataCount;
				else
					return 0;
			}
		}

		public override bool Check ()
		{
			return this.count >= PacketSizeSpecificMin
				&& base.Check ();
		}
	}
}
