// Ssm2ReadAddressesRequest.cs: SSM2 packet class for address(es) read request.

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

namespace Subaru.SSM2
{

	/// <summary>
	/// Packet to retrieve values for individual addresses.
	/// This packet type is usually created by the tester and sent to the car control unit.
	/// (Consists of 5 byte header + 1 padding + 3 * X addresses + 1 checksum = 7 + 3 * X.
	/// Minimum size is 10 for 1 address.)
	/// </summary>
	public sealed class Ssm2ReadAddressesRequest : Ssm2Packet
	{

		internal const int DefaultAddressesPerPacket = 36;
		static int maxAddresses = DefaultAddressesPerPacket;

		/// <summary>
		/// Maximum number of addresses to be used for a single packet.
		/// Defaults to 36 which most control units from year 2002+ should support.
		/// Possible theoretical range: 1 ≤ x ≤ 84.
		/// Control unit will not answer when using to many addresses!
		/// (Tested ok on modern cars (2008+): ≤ 82, packet size is 253 bytes for 82)
		/// (2005 cars might support ≤ 45.)
		/// (84 is theoretical limit because of packet length byte)
		/// </summary>
		public static int MaxAddressesPerPacket {
			get { return maxAddresses; }
			set {
				// Net payload = 255 payload length - 1 command byte - 1 padding byte = 253 bytes.
				// 253 / 3 = 84.333 => 84
				if (value < 1 || value > 84)
					throw new ArgumentOutOfRangeException ("value", value, "1 ≤ value ≤ 84");
				maxAddresses = value;
			}
		}

		/// <summary>
		/// Calculates packet size for this packet type.
		/// </summary>
		/// <returns>
		/// Total packet size.
		/// </returns>
		/// <param name='addressesCount'>
		/// Number of addresses to be used.
		/// </param>
		private static int PacketSize (int addressesCount)
		{
			return HeaderLength + 2 + 3 * addressesCount;
		}

		// require at least one address
		private const int PacketSizeSpecificMin = HeaderLength + 2 + 3 * 1;
		private const int IndexAddresses = HeaderLength + 1;


		#region constructors

		public Ssm2ReadAddressesRequest ()
		{
			// base class constructor will be called anyway
			this.count = PacketSizeSpecificMin;
		}

		public Ssm2ReadAddressesRequest (byte[] buffer) : base (buffer, PacketSizeSpecificMin)
		{
		}

		/// <summary>
		/// Constructs a complete packet.
		/// (Uses optimal buffer size)
		/// </summary>
		/// <param name="destination">
		/// A <see cref="SsmAddress"/>
		/// </param>
		/// <param name="source">
		/// A <see cref="SsmAddress"/>
		/// </param>
		/// <param name="addresses">
		/// An <see cref="IList<System.Int32>".
		/// Memory address(es) to request. (SSM2 protocol only uses 24 bits.)/>
		/// </param>
		public Ssm2ReadAddressesRequest (Ssm2Device destination,
		                                 Ssm2Device source,
		                                 IList<int> addresses)
			: base (PacketSize (addresses.Count))
		{
			this.Destination = destination;
			this.Source = source;
			// setting Addresses also updates count
			this.Addresses = addresses;

			this.propsSet = SetProperties.AllButChecksum;
			this.Finish ();
		}


		#endregion constructors


		// Implementation notes:
		// Returning IList<int> instead of IEnumerable<int> because of performance benefits
		// for following analysis (e.g. LINQ queries).

		/// <summary>
		/// Get/set address(es) to request.
		/// (SSM2 protocol only uses 24 bits.)
		/// Get may return empty list.
		/// Set needs at least one item.
		/// </summary>
		public IList<int> Addresses {
			get {
				int adrCount = AddressesCount;
				List<int> addresses = new List<int> (adrCount);
				for (int adrNr = 0; adrNr < adrCount; adrNr++) {
					addresses.Add (GetAddress (IndexAddresses + 3 * adrNr));
				}
				return addresses;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ();
				int adrCount = value.Count;
				if (adrCount <= 0 || adrCount > MaxAddressesPerPacket)
					throw new ArgumentOutOfRangeException ("value.Count", adrCount, "1 ≤ x ≤ " + MaxAddressesPerPacket.ToString ());

				int neededSize = PacketSize (adrCount);
				byte[] buf = this.buffer;
				if (buf.Length < neededSize) {
					// have to allocate bigger buffer and copy header data
					byte[] newBuffer = new byte[neededSize];
					Array.Copy (buf, newBuffer, IndexAddresses);
					this.buffer = newBuffer;
				}

				for (int n = 0; n < adrCount; n++) {
					int adr = value[n];
					AssertAddress (adr);
					SetAddress (adr, IndexAddresses + 3 * n);
				}

				// packet size known here
				this.count = neededSize;
				UpdateFlags (SetProperties.DataAll);
			}
		}

		/// <summary>
		/// Calculates the number of supplied addresses based on packet size.
		/// Returns 0 if packet size does not match with expected number of addresses.
		/// </summary>
		public int AddressesCount {
			get {
				int adrBytes = this.count - HeaderLength - 2;
				int adrCount = adrBytes / 3;
				return (adrBytes % 3 == 0 && adrCount > 0) ? adrCount : 0;
			}
		}

		public override bool Check ()
		{
			return this.Command == Ssm2Command.ReadAddressesRequestA8
				&& base.Check ();
		}

		/// <summary>
		/// Sets specific packet bytes.
		/// </summary>
		protected override void SetConstBytes ()
		{
			base.Command = Ssm2Command.ReadAddressesRequestA8;
			// 6th byte, following header, is padding 0x00
			buffer[HeaderLength] = 0;
		}
	}
}
