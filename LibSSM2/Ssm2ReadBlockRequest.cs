// Ssm2ReadBlockRequest.cs: SSM2 packet class for block read request.

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
	/// Read a block of data.
	/// Specific properties are starting address and data count.
	/// This packet type is usually created by the tester and sent to the car control unit.
	/// Always consists of 11 bytes: 5 header + 1 padding + 3 address + 1 count + 1 checksum byte.
	/// </summary>
	public sealed class Ssm2ReadBlockRequest : Ssm2Packet
	{
		private const int PacketSizeSpecificFixed = HeaderLength + 1 + 3 + 1 + 1;
		private const int IndexAddress = HeaderLength + 1;
		private const int IndexDataCount = IndexAddress + 3;

		#region constructors

		/// <summary>
		/// Creates an empty block read packet.
		/// (Allocated optimal 11 bytes buffer.)
		/// </summary>
		public Ssm2ReadBlockRequest () : base (PacketSizeSpecificFixed)
		{
			// this type of packet has fixed length so it can be set once here
			this.count = PacketSizeSpecificFixed;
		}

		/// <summary>
		/// Takes given (empty or preset) buffer.
		/// Needs at least 11 bytes.
		/// For already preset packet data call FromBytes(buffer)
		/// afterwards to set correct state.
		/// </summary>
		/// <param name="buffer">
		/// A <see cref="System.Byte[]"/>
		/// </param>
		public Ssm2ReadBlockRequest (byte[] buffer) : base (buffer, PacketSizeSpecificFixed)
		{
		}

		/// <summary>
		/// Creates a complete packet.
		/// (Will have optimal buffer length.)
		/// </summary>
		public Ssm2ReadBlockRequest (Ssm2Device destination,
		                             Ssm2Device source,
		                             int address,
		                             byte dataCount) : base (PacketSizeSpecificFixed)
		{
			this.Destination = destination;
			this.Source = source;
			this.Address = address;
			this.DataCount = dataCount;
			this.Finish ();
		}


		#endregion constructors

		public int Address {
			get {
				return GetAddress (IndexAddress);
			}
			set {
				AssertAddress (value);
				SetAddress (value, IndexAddress);
				UpdateFlags (SetProperties.Data1);
			}
		}

		/// <summary>
		/// Number of bytes to request.
		/// (Corresponding packet byte = count - 1 !
		/// E.g. packet byte 0x00 means 1 byte.)
		/// </summary>
		public byte DataCount {
			get { return (byte)(buffer[IndexDataCount] + 1); }
			set {
				buffer[IndexDataCount] = (byte)(value - 1);
				UpdateFlags (SetProperties.Data2 | SetProperties.Data3);
			}
		}

		public override bool Check ()
		{
			return this.Command == Ssm2Command.ReadBlockRequestA0
				&& this.count == PacketSizeSpecificFixed
				&& base.Check ();
		}

		/// <summary>
		/// Create a complete packet out of supplied data.
		/// </summary>
		protected override void SetConstBytes ()
		{
			base.Command = Ssm2Command.ReadBlockRequestA0;
			// 6th byte, following header, is padding = 0
			buffer[HeaderLength] = 0;
		}
	}
}
