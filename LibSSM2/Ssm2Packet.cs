// Ssm2Packet.cs: SSM2 packet base class.

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
using System.Diagnostics;

namespace Subaru.SSM2
{
	// Not working in MonoDevelop ?
	//[DebuggerDisplay("Size={Size}|Check()={Check()}")]
	//[DebuggerDisplay("Size={count}")]

	/// <summary>
	/// Subaru SSM2 packet base class.
	/// Designed for object reusability which means immutable.
	/// Properties can be changed and will only affect those packet bytes.
	/// I.e. once header info has been set, one can modify payload data, header bytes won't be rewritten.
	/// Also avoids many backing fields since most properties directly read/write packet bytes.
	/// </summary>
	public class Ssm2Packet : ISsm2Packet
	{
		// Implementation notes: This class has no abstract members
		// in order to allow this class to be used as concrete base class.
		// May want to get common info out of any packet object.

		/// <summary>
		/// To record which properties have already been set.
		/// Will be checked by the Finish() method.
		/// </summary>
		[Flags]
		protected enum SetProperties
		{
			Source = 1,
			Destination = 2,
			/// <summary>
			/// DataX for specific packet types. All Data bits must be set.
			/// If a packet has less than 3 data properties, combine DataX accordingly as DataAll will be checked.
			/// </summary>
			Data1 = 16,
			Data2 = 32,
			Data3 = 64,
			DataAll = Data1 | Data2 | Data3,
			Checksum = 128,
			AllButChecksum = Source | Destination | DataAll,
			AllOk = Source | Destination | DataAll | Checksum
		}


		/// <summary>
		/// Returns a specific Ssm2Packet type based on given content.
		/// Does not validate the packet, so manually call the Check method afterwards!
		/// </summary>
		/// <param name="bytes">
		/// A <see cref="System.Byte[]"/> containing the packet.
		/// The packet must start at index 0 though the array may be larger than needed.
		/// </param>
		/// <returns>
		/// A <see cref="Ssm2Packet"/> subclass or null if the packet type could not be recognized.
		/// </returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Ssm2Packet NewFromBytes (byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException ();
			// check generic minimum size in order to get command byte
			if (bytes.Length < PacketSizeMin)
				throw new ArgumentOutOfRangeException ("bytes.Length < Minimum (6)");

			Ssm2Packet p;
			// Read type directly from command byte (5th byte).
			// Each derived class constructor checks its requirements.
			switch ((Ssm2Command)bytes[(int)Ssm2PacketIndex.Command]) {
			case Ssm2Command.ReadAddressesRequestA8:
				p = new Ssm2ReadAddressesRequest (bytes);
				break;
			case Ssm2Command.ReadAddressesResponseE8:
				p = new Ssm2ReadAddressesResponse (bytes);
				break;
			case Ssm2Command.WriteAddressRequestB8:
				p = new Ssm2WriteAddressRequest (bytes);
				break;
			case Ssm2Command.WriteAddressResponseF8:
				p = new Ssm2WriteAddressResponse (bytes);
				break;
			case Ssm2Command.InitRequestBF:
				p = new Ssm2InitRequest (bytes);
				break;
			case Ssm2Command.InitResponseFF:
				p = new Ssm2InitResponse (bytes);
				break;
			case Ssm2Command.ReadBlockRequestA0:
				p = new Ssm2ReadBlockRequest (bytes);
				break;
			case Ssm2Command.ReadBlockResponseE0:
				p = new Ssm2ReadBlockResponse (bytes);
				break;
			default:
				return null;
			}
			p.FromBytes (bytes);
			return p;
		}



		/// <summary>
		/// Thoretical SSM2 packet size limit (because of maximum length byte) is
		/// 260 bytes: 4 header bytes incl. data size byte + 255 bytes content + checksum.
		/// (Tests on modern cars indicate actual limit of 255!)
		/// </summary>
		public const int PacketSizeMax = 4 + 255 + 1;
		// A packet has at least 6 bytes (5 Header bytes + 1 checksum byte)
		public const int PacketSizeMin = HeaderLength + 1;

		/// <summary>
		/// All SSM2 packets start with this byte (128 = 0x80)
		/// </summary>
		private const byte FirstByte = 0x80;

		/// <summary>
		/// Number of SSM2 header pytes is 5,
		/// = minimum packet size excluding checksum.
		/// </summary>
		protected const int HeaderLength = 5;

		#region fields

		// Implementation note:
		// Not using List<T> as storage
		// because cannot reuse an existing buffer as
		// there is no List-constructor taking an existing array.
		// Therefore using a byte[] array plus a count field instead.

		/// <summary>
		/// Storage to use, make sure it is big enough.
		/// Use constant MaxBytesPacket.
		/// </summary>
		protected byte[] buffer;

		// hide in debugger since shown via property anyway
		/// <summary>
		/// Currently used packet size.
		/// Can be smaller than buffer size in order to (re)use a bigger buffer.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected int count;

		protected SetProperties propsSet;

		#endregion fields

		#region constructors

		/// <summary>
		/// Creates a SSM2 packet.
		/// Afterwards you have to set all properties and
		/// call Finish() to complete the packet.
		/// (This constructor allocates maximum possible packet size of 260 bytes.)
		/// </summary>
		public Ssm2Packet () : this (PacketSizeMax)
		{
			// set default size
			this.count = PacketSizeMin;
			SetFirstByte ();
			// If used from derived classes then
			// this is calling the overriden method!!!
			this.SetConstBytes ();
		}

		/// <summary>
		/// Takes given (empty) storage buffer and sets first byte.
		/// Afterwards you have to set all properties and
		/// call Construct() to finish the packet.
		/// This constructor is not meant for parsing an existing packet,
		/// use static method NewFromBytes instead or
		/// instance method FromBytes instead!
		/// </summary>
		/// <param name="buffer">
		/// A <see cref="System.Byte[]"/>. Minimum
		/// </param>
		/// <exception cref='ArgumentOutOfRangeException'>
		/// If buffer.Length < minimum (6).
		/// </exception>
		public Ssm2Packet (byte[] buffer) : this (buffer, PacketSizeMin)
		{
		}

		/// <summary>
		/// Assures minimum capacity of given buffer.
		/// Useful when specific minimum or fixed packet size is known.
		/// Also sets packet size to given capacity.
		/// </summary>
		protected Ssm2Packet (byte[] buffer, int capacityMin)
		{
			TakeBuffer (buffer, capacityMin);
			this.count = capacityMin;
			SetFirstByte ();
			this.SetConstBytes ();
		}

		/// <summary>
		/// Creates a new packet with specified capacity.
		/// Afterwards you have to set all properties and
		/// call Construct() to finish the packet.
		/// </summary>
		/// <param name='capacity'>
		/// Maximum packet size.
		/// </param>
		/// <exception cref='ArgumentOutOfRangeException'>
		/// Capacity ≥ 6 required.
		/// </exception>
		public Ssm2Packet (int capacity)
		{
			if (capacity < PacketSizeMin)
				throw new ArgumentOutOfRangeException ("buffer.Length",
					capacity, "< minimum of " + PacketSizeMin.ToString());

			this.buffer = new byte[capacity];
			this.count = PacketSizeMin;
			SetFirstByte ();
			this.SetConstBytes ();
		}

		#endregion constructors


		/// <summary>
		/// Parses packet by taking packet bytes, does not use a copy!
		/// Assumes complete packet, does not validate.
		/// May want to call Check () afterwards.
		/// (Packet size is being calculated based on packet length byte.)
		/// </summary>
		/// <param name="buffer">
		/// A <see cref="System.Byte[]"/>. Length can be larger than needed.
		/// </param>
		/// <exception cref='ArgumentNullException'>
		/// </exception>
		/// <exception cref='ArgumentOutOfRangeException'>
		/// Length ≥ 6 required.
		/// </exception>
		public virtual void FromBytes (byte[] buffer)
		{
			TakeBuffer (buffer, PacketSizeMin);
			// Calculate packet size based on DataSize byte so given byte[] can be larger than needed.
			// total count = DataSize + remaining header bytes + checksum byte
			this.count = this.PayloadSize + ((int)Ssm2PacketIndex.DataSize + 1 + 1);
			if (buffer.Length < this.count)
				throw new ArgumentOutOfRangeException ("buffer.Length < expected");
			// Assume packet to be complete incl. checksum.
			this.propsSet = SetProperties.AllOk;
		}

		/// <summary>
		/// Returns a copy of the actual packet bytes.
		/// May return empty array or less bytes if packet is incomplete.
		/// Guaranteed to not return null.
		/// Use method Check () before to assure validity.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Byte[]"/>.
		/// </returns>
		public byte[] ToBytesCopy ()
		{
			int count = this.count;
			byte[] copy = new byte[count];
			Array.Copy (this.buffer, copy, count);
			return copy;
		}

		/// <summary>
		/// Command.
		/// (For debugging mostly, = 5th packet byte.)
		/// </summary>
		public Ssm2Command Command {
			get { return (Ssm2Command)buffer[(int)Ssm2PacketIndex.Command]; }
			protected set {
				this.buffer[(int)Ssm2PacketIndex.Command] = (byte)value;
			}
		}

		public Ssm2Device Source {
			get { return (Ssm2Device)buffer[(int)Ssm2PacketIndex.Source]; }
			set {
				if (value == this.Source)
					return;
				this.buffer[(int)Ssm2PacketIndex.Source] = (byte)value;
				UpdateFlags (SetProperties.Source);
			}
		}

		public Ssm2Device Destination {
			get { return (Ssm2Device)buffer[(int)Ssm2PacketIndex.Destination]; }
			set {
				if (value == this.Destination)
					return;
				this.buffer[(int)Ssm2PacketIndex.Destination] = (byte)value;
				UpdateFlags (SetProperties.Destination);
			}
		}

		/// <summary>
		/// Currently used packet size.
		/// May be less than capacity.
		/// For debugging purposes mostly.
		/// Valid for valid packet only.
		/// </summary>
		public int Size {
			get { return this.count; }
		}

		/// <summary>
		/// Report currently allocated buffer size.
		/// (For debugging purposes mostly.)
		/// </summary>
		/// <value>
		/// Should be ≥ minimum (6) for all packet types.
		/// </value>
		public int Capacity {
			// constructors and methods should assure buffer != null
			get { return this.buffer.Length; }
			// get { return this.buffer != null ? this.buffer.Length : 0; }
		}

		/// <summary>
		/// Calculate and compare checksum.
		/// </summary>
		/// <returns>true if checksum is ok</returns>
		public bool IsChecksumOk {
			// last byte is always checksum
			get { return (this.buffer[this.count - 1] == this.ChecksumCalculated); }
		}

		// Can be useful in debugger, therefore implemented as property getter.
		/// <summary>
		/// All bytes till before checksum (incl. header) are used for checksum.
		/// Assumes Count is total packet length including checksum byte.
		/// (For debugging purposes mostly.)
		/// </summary>
		public byte ChecksumCalculated {
			get {
				// Checksum is lowest byte of sum
				// beginning with first packet byte,
				// excluding last byte (checksum itself).
				byte[] b = this.buffer;
				int s = 0;
				int m = this.count - 1;
				for (int i = 0; i < m; i++)
					s += b[i];

				// not necessary: (byte)(s & 0xFF)
				return (byte)s;
			}
		}

		// To be overriden in order to include checking of type specific features.
		/// <summary>
		/// Validate packet completely including checksum.
		/// Checks common and packet type specific characteristics.
		/// </summary>
		/// <returns>true if everything is OK.</returns>
		public virtual bool Check ()
		{
			int c = this.count;
			return (propsSet == SetProperties.AllOk
			        && buffer[(int)Ssm2PacketIndex.Header] == FirstByte
			        && c >= PacketSizeMin
			        && PayloadSize == c - HeaderLength
			        && Destination != Source
			        && IsChecksumOk);
		}


		/// <summary>
		/// Finish the packet.
		/// (Checksum and inline length byte.)
		/// Also checks if all properties have been set.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Not all required properties have been set.</exception>
		public void Finish ()
		{
			// if all set or nothing has changed then there is nothing to do
			if (propsSet == SetProperties.AllOk)
				return;

			if (propsSet != SetProperties.AllButChecksum) {
				SetProperties missing = (SetProperties)(SetProperties.AllButChecksum - this.propsSet);
				throw new InvalidOperationException ("Properties not set: " + missing.ToString ());
			}

			// Assumes full packet size incl. checksum!
			// optimize using local copy
			int count = this.count;

			// set data size byte (4th byte), = total packet size - 5
			// counting bytes from command (5th) byte till end, excluding last byte = checksum
			buffer[(int)Ssm2PacketIndex.DataSize] =
				(byte)(count - ((int)Ssm2PacketIndex.DataSize + 2));

			// reserve last byte for checksum, checksum method assumes Count of full packet
			buffer[count - 1] = ChecksumCalculated;
			this.propsSet = SetProperties.AllOk;
		}

		// overriding Equals, GetHashCode etc. not necessary
		// but provide much better performance if used

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			Ssm2Packet p = obj as Ssm2Packet;
			if (p == null)
				return false;

			// Return true if the data matches:
			return (this.GetHashCode () == p.GetHashCode ());
		}

		public bool Equals(Ssm2Packet p)
		{
			if (p == null)
				return false;
			return (this.GetHashCode () == p.GetHashCode ());
		}

		public override int GetHashCode()
		{
			// Tested: buffer.GetHashCode () does
			// not produce hash based on content!
			// Arrays with same size and content yield different hash!

			// The single SSM2 checksum byte is not sufficient
			// as many different packets can have same checksum.
			// XOR
			return this.count ^ this.ChecksumCalculated;
		}


		#region private/protected

		/// <summary>
		/// Get payload length byte (4th header byte).
		/// Payload consists of command byte (5th) to last-1 (excluding checksum) byte.
		/// Therefore payload is total packet length minus 5 (4 + 1).
		/// </summary>
		protected byte PayloadSize {
			get { return this.buffer[(int)Ssm2PacketIndex.DataSize]; }
		}

		// Private access is sufficient since derived class
		// constructors should call a base class constructor.
		private void SetFirstByte ()
		{
			// 1st packet byte is always 128 = 0x80
			this.buffer[(int)Ssm2PacketIndex.Header] = FirstByte;
		}

		/// <summary>
		/// Sets the const bytes except first packet byte.
		/// (Must be overriden by derived classes
		/// to set command and possible padding bytes.)
		/// </summary>
		protected virtual void SetConstBytes ()
		{
		}

		/// <summary>
		/// Set flag to indicate that this part of the packet has been set.
		/// Also unsets checksum flag as it needs recalculation in method Finish().
		/// </summary>
		/// <param name='toSet'>
		/// The flag to set.
		/// </param>
		protected void UpdateFlags (SetProperties toSet)
		{
			SetProperties props = this.propsSet;
			// set flag
			props |= toSet;

			// unset checksum flag
			// "~": bitwise complement operation on its operand,
			//      which has the effect of reversing each bit
			props &= ~SetProperties.Checksum;

			this.propsSet = props;
		}

		/// <summary>
		/// Takes the buffer and checks for minimum length.
		/// </summary>
		/// <param name='buffer'>
		/// Buffer.
		/// </param>
		/// <param name='minLength'>
		/// Minimum buffer length.
		/// </param>
		/// <exception cref='ArgumentNullException'>
		/// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
		/// </exception>
		/// <exception cref='ArgumentOutOfRangeException'>
		/// If buffer.Length &lt; minimum.
		/// </exception>
		protected void TakeBuffer (byte[] buffer, int minLength)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (buffer.Length < minLength)
				throw new ArgumentOutOfRangeException ("buffer.Length",
					buffer.Length, "must be ≥ " + minLength.ToString ());
			this.buffer = buffer;
			// set size to min length for now
			this.count = minLength;
		}

		// TODO Could be stripped in Release version
		//[System.Diagnostics.Conditional("DEBUG")]
		// Might just use 24 bits without complaining.
		// Currently used by Ssm2WriteAddressRequest and Ssm2ReadBlockRequest
		/// <summary>
		/// Throw ArgumentOutOfRangeException if outside valid range: 0 ≤ address ≤ 0xFFFFFF.
		/// (SSM2 addresses consist of 3 bytes allowing 16 MiB address space.)
		/// </summary>
		/// <param name="address">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <exception cref="System.ArgumentOutOfRangeException"></exception>
		static protected void AssertAddress (int address)
		{
			if (address < 0 || address > 0xffffff)
				throw new ArgumentOutOfRangeException ("address",
					address, "0 ≤ address ≤ 0xFFFFFF");
		}

		// common functionality for some derived types
		protected int GetAddress (int index)
		{
			// optimize using local variable
			byte[] buffer = this.buffer;
			// big endian = most significant byte first
			// 3 bytes per address = 24 bits address space
			return (buffer[index++] << 16) + (buffer[index++] << 8) + buffer[index++];
		}

		// common functionality for some derived types
		protected void SetAddress (int address, int index)
		{
			// optimize using local variable
			byte[] buffer = this.buffer;
			// big endian = most significant byte first
			// 3 bytes per address = 24 bits address space
			buffer[index++] = (byte)(address >> 16);
			buffer[index++] = (byte)(address >> 8);
			buffer[index++] = (byte)address;
		}


		#endregion private/protected
	}
}
