// Ssm2InitResponse.cs: SSM2 packet class for init response.

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
	/// Init response packet.
	/// This packet type is usually created by control unit and sent to the tester.
	/// Packet consists of: 5 byte header + 3 SSMID + 5 ROMID + X capability bytes + 1 checksum.
	/// Capability bytes length is not constant, usually 32, 48 or 96 bytes seen so far.
	/// </summary>
	public sealed class Ssm2InitResponse : Ssm2Packet
	{
		const int LengthSsmID = 3;
		const int LengthRomID = 5;
		// TODO Research actual capability bytes minimum of oldest SSM2 cars.
		// Probably 16 or 32, rather not 0.
		const int CapabilitiesMin = 0;
		const int CapabilitiesMax = PacketSizeMax - PacketSizeSpecificMin;

		const int PacketSizeSpecificMin = HeaderLength + LengthSsmID + LengthRomID + CapabilitiesMin + 1;

		const int IndexSsmID = HeaderLength;
		const int IndexRomID = IndexSsmID + LengthSsmID;
		const int IndexCapabilities = IndexRomID + LengthRomID;

		#region constructors

		public Ssm2InitResponse ()
		{
			this.count = PacketSizeSpecificMin;
		}

		public Ssm2InitResponse (byte[] buffer) : base (buffer, PacketSizeSpecificMin)
		{
		}

		// TODO argument checking
		/// <summary>
		/// Creates a complete InitResponse packet.
		/// </summary>
		public Ssm2InitResponse (Ssm2Device destination,
		                         Ssm2Device source,
		                         byte[] ssmID,
		                         byte[] romID,
		                         byte[] capabilities)
			: base (capabilities.Length + (HeaderLength + LengthSsmID + LengthRomID + 1))
		{
			this.Destination = destination;
			this.Source = source;

			SetSsmID (ssmID);
			SetRomID (romID);
			// setting capability bytes also updates size
			SetCapabilities (capabilities);

			this.Finish ();
		}


		#endregion constructors

		#region data access

		// Implementation notes:
		// Since array implements IList<T>, returning an array seems
		// to be both versatile and efficient.
		// But properties (normally) should not return an array,
		// therefore using plain methods instead.

		/// <summary>
		/// Return a copy of the 3 SSMID bytes.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Byte[]"/>
		/// </returns>
		public byte[] GetSsmID ()
		{
			return CopyPart (IndexSsmID, LengthSsmID);
		}

		/// <summary>
		/// Sets SSMID
		/// </summary>
		/// <param name='ssmID'>
		/// Always 3 bytes e.g. "{0xA2, 0x10, 0x12}".
		/// </param>
		/// <exception cref='ArgumentNullException'></exception>
		/// <exception cref='ArgumentOutOfRangeException'>Array length must be 3.</exception>
		public void SetSsmID (byte[] ssmID)
		{
			TakePart (ssmID, IndexSsmID, LengthSsmID);
			UpdateFlags (SetProperties.Data1);
		}

		/// <summary>
		/// Returns a copy of the 5 ROMID bytes.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Byte[]"/>
		/// </returns>
		public byte[] GetRomID ()
		{
			return CopyPart (IndexRomID, LengthRomID);
		}

		/// <summary>
		/// Sets ROMID.
		/// </summary>
		/// <param name='romID'>
		/// Always 5 bytes e.g. "{0x1B, 0x14, 0x40, 0x05, 0x05}".
		/// </param>
		/// <exception cref='ArgumentNullException'></exception>
		/// <exception cref='ArgumentOutOfRangeException'>Array length must be 5.</exception>
		public void SetRomID (byte[] romID)
		{
			TakePart (romID, IndexRomID, LengthRomID);
			UpdateFlags (SetProperties.Data2);
		}

		/// <summary>
		/// Get the number of capability bytes.
		/// (Calculated via packet size.
		/// Usually 32, 48 or 96 bytes seen so far.)
		/// </summary>
		public int CapabilitiesLength {
			get {
				// capability bytes are the only variable content
				int length = this.count - PacketSizeSpecificMin;
				return (length > 0 && length <= CapabilitiesMax) ? length : 0;
			}
		}

		/// <summary>
		/// Returns a copy of the capability bytes.
		/// (Lengths of 32 = 0x20, 48 = 0x30 or 96 = 0x60 bytes seen so far.)
		/// </summary>
		/// <returns>
		/// A <see cref="System.Byte[]"/>
		/// </returns>
		public byte[] GetCapabilities ()
		{
			return CopyPart (IndexCapabilities, CapabilitiesLength);
		}

		/// <summary>
		/// Set capability bytes.
		/// </summary>
		/// <param name='capabilities'>
		/// Capabilities.
		/// </param>
		/// <exception cref='ArgumentNullException'></exception>
		/// <exception cref='ArgumentOutOfRangeException'>
		/// Length out of range. Must be greater than 1.
		/// </exception>
		public void SetCapabilities (byte[] capabilities)
		{
			if (capabilities == null)
				throw new ArgumentNullException ();
			int length = capabilities.Length;
			if (length < CapabilitiesMin || length > CapabilitiesMax)
				throw new ArgumentOutOfRangeException ("capabilities.Length",
					length,
					CapabilitiesMin.ToString() + " ≤ x ≤ " + CapabilitiesMax.ToString());
			Array.Copy (capabilities, 0, this.buffer, IndexCapabilities, length);
			UpdateFlags (SetProperties.Data3);

			// size known here
			this.count = PacketSizeSpecificMin + length;
		}

		#endregion data access

		public override bool Check ()
		{
			return this.Command == Ssm2Command.InitResponseFF
				&& this.count >= PacketSizeSpecificMin
				&& base.Check ();
		}

		protected override void SetConstBytes ()
		{
			base.Command = Ssm2Command.InitResponseFF;
			// no padding byte
		}

		private byte[] CopyPart (int index, int length)
		{
			byte[] bytes = new byte[length];
			Array.Copy (this.buffer, index, bytes, 0, length);
			return bytes;
		}

		private void TakePart (byte[] bytes, int index, int length)
		{
			if (bytes == null)
				throw new ArgumentNullException ();
			if (bytes.Length != length)
				throw new ArgumentOutOfRangeException (
					".Length", bytes.Length, "must be " + length.ToString());
			Array.Copy (bytes, 0, this.buffer, index, length);
		}
	}
}
