// ISsm2Packet.cs: Interface that all SSM2 packet classes have to implement.

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

namespace Subaru.SSM2
{

	/// <summary>
	/// Specifies common SSM2 packet features.
	/// </summary>
	public interface ISsm2Packet
	{
		Ssm2Device Destination { get; set; }
		Ssm2Device Source { get; set; }
		Ssm2Command Command { get; }

		/// <summary>
		/// Currently used packet size.
		/// May be less than capacity.
		/// Valid for valid packet content only.
		/// (For debugging purposes mostly.)
		/// </summary>
		int Size { get; }

		/// <summary>
		/// Report currently allocated buffer size.
		/// (For debugging purposes mostly.)
		/// </summary>
		/// <value>
		/// Should be ≥ minimum (6).
		/// </value>
		int Capacity { get; }

		/// <summary>
		/// Validates packet including checksum.
		/// Checks both common and packet type specific characteristics.
		/// </summary>
		/// <returns>true if everything is OK.</returns>
		bool Check ();

		// Can be useful in debugger, therefore implemented as property getter.
		/// <summary>
		///  All bytes till before checksum (incl. header) are used for checksum.
		///  Assumes Count is total packet length including checksum byte.
		/// (For debugging purposes mostly.)
		/// </summary>
		byte ChecksumCalculated { get; }

		/// <summary>
		/// Calculate and compare checksum.
		/// </summary>
		/// <returns>true if checksum is ok</returns>
		bool IsChecksumOk { get; }

		void FromBytes (byte[] bytes);

		/// <summary>
		/// Finish the packet.
		/// (Checksum and inline length byte.)
		/// </summary>
		void Finish ();

		byte[] ToBytesCopy ();
	}
}
