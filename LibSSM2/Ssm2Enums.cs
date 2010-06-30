// Ssm2Enums.cs: Public enums to be used for SSM2 types.

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


namespace Subaru.SSM2
{

	/// <summary>
	/// SSM2 command identifier.
	/// (Last two hex characters represent actual packet byte, useful for debugging.)
	/// </summary>
	public enum Ssm2Command : byte
	{
		None = 0,
		ReadBlockRequestA0 = 0xa0,
		ReadBlockResponseE0 = 0xe0,
		ReadAddressesRequestA8 = 0xa8,
		ReadAddressesResponseE8 = 0xe8,
		WriteBlockRequestB0 = 0xb0,
		WriteBlockResponseF0 = 0xf0,
		WriteAddressRequestB8 = 0xb8,
		WriteAddressResponseF8 = 0xf8,
		InitRequestBF = 0xbf,
		InitResponseFF = 0xff
	}

	/// <summary>
	/// SSM2 device ID.
	/// (Last two hex characters represent actual packet byte, useful for debugging.)
	/// Many different IDs for various control units may be valid,
	/// depending on the model.
	/// You can always provide own/undefined values by casting
	/// like this: (Ssm2Device)0x123.
	/// </summary>
	public enum Ssm2Device : byte
	{
		None = 0,
		Engine10 = 0x10,
		Transmission18 = 0x18,
		DiagnosticToolF0 = 0xf0
	}

	/// <summary>
	/// Well-known constant buffer indices in any SSM2 packet.
	/// (Mostly for internal and testing purposes.)
	/// </summary>
	public enum Ssm2PacketIndex
	{
		/// <summary>
		/// This first byte is always supposed to be 128 = 0x80.
		/// </summary>
		Header,
		/// <summary>
		/// Destination device.
		/// Typically 0x10 = engine, 0x18 = transmission, 0xF0 = diagnostic tool.
		/// </summary>
		Destination,
		/// <summary>
		/// Source device. Same principle as Destination.
		/// </summary>
		Source,
		/// <summary>
		/// Inline payload length, counting all following bytes except checksum.
		/// </summary>
		DataSize,
		Command,
		/// <summary>
		/// Generic data, length varies.
		/// Some packet types use a padding byte as first data byte.
		/// </summary>
		Data
	}
}
