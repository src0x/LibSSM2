// Ssm2WriteAddressRequest.cs: SSM2 packet class for address write request.

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
	/// Write a single byte into an address.
	/// This packet type is usually created by the tester and sent to the car control unit.
	/// (Packet always consists of 10 bytes: 5 byte header + 3 address bytes + 1 data byte + 1 checksum.)
	/// </summary>
	public sealed class Ssm2WriteAddressRequest : Ssm2Packet
	{
		/// <summary>
		/// Packet always consists of 10 bytes: 5 byte header + 3 address bytes + 1 data byte + 1 checksum.
		/// </summary>
		private const int PacketSizeSpecificFixed = HeaderLength + 3 + 1 + 1;

		#region constructors

		public Ssm2WriteAddressRequest ()
		{
			// base class constructor will be called anyway
			//  : base()
			// this type of packet has fixed length
			this.count = PacketSizeSpecificFixed;
		}

		public Ssm2WriteAddressRequest (byte[] buffer)
			: base (buffer, PacketSizeSpecificFixed)
		{
		}

		/// <summary>
		/// Uses optimal buffer size.
		/// </summary>
		/// <param name="destination">
		/// A <see cref="SsmAddress"/>
		/// </param>
		/// <param name="source">
		/// A <see cref="SsmAddress"/>
		/// </param>
		/// <param name="addresses">
		/// A <see cref="IList<System.Int32>"/>
		/// </param>
		public Ssm2WriteAddressRequest (Ssm2Device destination,
		                                Ssm2Device source,
		                                int address,
		                                byte data)
			: base (PacketSizeSpecificFixed)
		{
			this.count = PacketSizeSpecificFixed;
			this.Destination = destination;
			this.Source = source;
			this.Address = address;
			this.Data = data;
		}


		#endregion constructors

		public int Address {
			get {
				return GetAddress (HeaderLength);
			}
			set {
				AssertAddress (value);
				SetAddress (value, HeaderLength);
				UpdateFlags (SetProperties.Data1);
			}
		}

		public byte Data {
			get { return this.buffer[HeaderLength + 3]; }
			set {
				this.buffer[HeaderLength + 3] = value;
				UpdateFlags (SetProperties.Data2 | SetProperties.Data3);
			}
		}

		public override bool Check ()
		{
			return this.Command == Ssm2Command.WriteAddressRequestB8
				&& this.count == PacketSizeSpecificFixed
				&& base.Check ();
		}

		protected override void SetConstBytes ()
		{
			// 5th byte = command
			base.Command = Ssm2Command.WriteAddressRequestB8;
		}
	}
}
