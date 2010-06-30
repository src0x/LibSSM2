// Ssm2WriteAddressResponse.cs: SSM2 packet class for address write response.

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
	/// Response packet of a write-address-request.
	/// Contains a single byte confirming the written byte.
	/// This packet type is usually created by the tester and sent to the car control unit.
	/// Example confirming data byte 0x02: { 0x80, 0xF0, 0x10, 0x02, 0xF8, 0x02, 0x7C }
	/// </summary>
	public sealed class Ssm2WriteAddressResponse : Ssm2Packet
	{
		/// <summary>
		/// Packet always consists of 7 bytes: 5 byte header + 1 data byte + 1 checksum.
		/// </summary>
		private const int PacketSizeSpecificFixed = HeaderLength + 1 + 1;


		#region constructors

		/// <summary>
		/// Uses optimal buffer size.
		/// </summary>
		public Ssm2WriteAddressResponse () : base (PacketSizeSpecificFixed)
		{
			// this type of packet has fixed length so it can be set once here
			this.count = PacketSizeSpecificFixed;
		}

		/// <summary>
		/// Takes given (empty or preset) buffer.
		/// Needs at least 7 bytes.
		/// For already preset packet data call FromBytes(buffer) afterwards to set correct state.
		/// </summary>
		/// <param name="buffer">
		/// A <see cref="System.Byte[]"/>
		/// </param>
		public Ssm2WriteAddressResponse (byte[] buffer)
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
		public Ssm2WriteAddressResponse (Ssm2Device destination,
		                                 Ssm2Device source,
		                                 byte data)
			: this ()
		{
			this.Destination = destination;
			this.Source = source;
			this.Data = data;

			this.Finish();
		}


		#endregion constructors


		public byte Data {
			get {
				return this.buffer[HeaderLength];
			}
			set {
				this.buffer[HeaderLength] = value;
				UpdateFlags (SetProperties.DataAll);
			}
		}

		public override bool Check ()
		{
			return this.Command == Ssm2Command.WriteAddressResponseF8
				&& this.count == PacketSizeSpecificFixed
				&& base.Check ();
		}

		protected override void SetConstBytes ()
		{
			base.Command = Ssm2Command.WriteAddressResponseF8;
			// no padding byte
		}
	}
}
