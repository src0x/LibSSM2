// Ssm2ReadBlockResponse.cs: SSM2 packet class for block read response.

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
	/// Returns requested data of a previous read-block-request.
	/// This packet type is usually created by the car control unit and sent to the tester.
	/// Example containing 3 data bytes: { 0x80, 0xF0, 0x10, 0x04, 0xE0, 0x01, 0x02, 0x03, 0x6A }.
	/// </summary>
	public sealed class Ssm2ReadBlockResponse : Ssm2DataResponse
	{
		#region constructors

		public Ssm2ReadBlockResponse () : base ()
		{
		}

		public Ssm2ReadBlockResponse (byte[] buffer) : base (buffer)
		{
		}

		public Ssm2ReadBlockResponse (Ssm2Device destination,
		                              Ssm2Device source,
		                              IList<byte> data)
			: base (destination, source, data)
		{
		}

		#endregion constructors

		public override bool Check ()
		{
			return this.Command == Ssm2Command.ReadBlockResponseE0
				&& base.Check ();
		}

		protected override void SetConstBytes ()
		{
			base.Command = Ssm2Command.ReadBlockResponseE0;
			// no padding
		}
	}
}
