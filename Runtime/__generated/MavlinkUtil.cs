using System;
using System.Linq;
using System.Runtime.InteropServices;


/// <summary>
/// Static methods and helpers for creation and manipulation of Mavlink packets
/// </summary>
public static class MavlinkUtil
{
    public static bool UseUnsafe { get; set; } = true;

    /// <summary>
    /// Create a new mavlink packet object from a byte array as received over mavlink
    /// Endianness will be detected using packet inspection
    /// </summary>
    /// <typeparam name="TMavlinkPacket">The type of mavlink packet to create</typeparam>
    /// <param name="bytearray">The bytes of the mavlink packet</param>
    /// <param name="startoffset">The position in the byte array where the packet starts</param>
    /// <returns>The newly created mavlink packet</returns>
    public static TMavlinkPacket ByteArrayToStructure<TMavlinkPacket>(this byte[] bytearray, int startoffset = 6)
        where TMavlinkPacket : struct
    {
        if (UseUnsafe)
            return ReadUsingPointer<TMavlinkPacket>(bytearray, startoffset);
        else
            return ByteArrayToStructureGC<TMavlinkPacket>(bytearray, startoffset);
    }

    public static TMavlinkPacket ByteArrayToStructureBigEndian<TMavlinkPacket>(this byte[] bytearray,
        int startoffset = 6) where TMavlinkPacket : struct
    {
        object newPacket = new TMavlinkPacket();
        ByteArrayToStructureEndian(bytearray, ref newPacket, startoffset);
        return (TMavlinkPacket)newPacket;
    }

    public static void ByteArrayToStructure(byte[] bytearray, ref object obj, int startoffset, int payloadlength = 0)
    {
        if (bytearray == null || bytearray.Length < startoffset + payloadlength || payloadlength == 0)
            return;

        var len = Marshal.SizeOf(obj);

        var iptr = IntPtr.Zero;

        try
        {
            iptr = Marshal.AllocHGlobal(len);
            //clear memory
            for (var i = 0; i < len / 8; i++) Marshal.WriteInt64(iptr, i * 8, 0x00);

            for (var i = len - len % 8; i < len; i++) Marshal.WriteByte(iptr, i, 0x00);

            // copy byte array to ptr
            Marshal.Copy(bytearray, startoffset, iptr, payloadlength);

            obj = Marshal.PtrToStructure(iptr, obj.GetType());
        }
        finally
        {
            if (iptr != IntPtr.Zero)
                Marshal.FreeHGlobal(iptr);
        }
    }

    public static TMavlinkPacket ByteArrayToStructureT<TMavlinkPacket>(byte[] bytearray, int startoffset)
    {
        if (bytearray == null || bytearray.Length < startoffset)
            return default;

        var len = bytearray.Length - startoffset;

        var i = Marshal.AllocHGlobal(len);

        try
        {
            // copy byte array to ptr
            Marshal.Copy(bytearray, startoffset, i, len);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ByteArrayToStructure FAIL " + ex.Message);
        }

        var obj = Marshal.PtrToStructure(i, typeof(TMavlinkPacket));

        Marshal.FreeHGlobal(i);

        return (TMavlinkPacket)obj;
    }

    public static byte[] trim_payload(ref byte[] payload)
    {
        var length = payload.Length;
        while (length > 1 && payload[length - 1] == 0) length--;
        if (length != payload.Length)
            Array.Resize(ref payload, length);
        return payload;
    }

    public static T ReadUsingPointer<T>(byte[] data, int startoffset) where T : struct
    {
        if (data == null || data.Length < startoffset)
            return default;
        unsafe
        {
            fixed (byte* p = &data[startoffset])
            {
                return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
            }
        }
    }

    public static T ByteArrayToStructureGC<T>(byte[] bytearray, int startoffset) where T : struct
    {
        var gch = GCHandle.Alloc(bytearray, GCHandleType.Pinned);
        try
        {
            return (T)Marshal.PtrToStructure(new IntPtr(gch.AddrOfPinnedObject().ToInt64() + startoffset), typeof(T));
        }
        finally
        {
            gch.Free();
        }
    }

    public static void ByteArrayToStructureEndian(byte[] bytearray, ref object obj, int startoffset)
    {
        var len = Marshal.SizeOf(obj);
        var i = Marshal.AllocHGlobal(len);
        var temparray = (byte[])bytearray.Clone();

        // create structure from ptr
        obj = Marshal.PtrToStructure(i, obj.GetType());

        // do endian swap
        var thisBoxed = obj;
        var test = thisBoxed.GetType();

        var reversestartoffset = startoffset;

        // Enumerate each structure field using reflection.
        foreach (var field in test.GetFields())
        {
            // field.Name has the field's name.
            var fieldValue = field.GetValue(thisBoxed); // Get value

            // Get the TypeCode enumeration. Multiple types get mapped to a common typecode.
            var typeCode = Type.GetTypeCode(fieldValue.GetType());

            if (typeCode != TypeCode.Object)
            {
                Array.Reverse(temparray, reversestartoffset, Marshal.SizeOf(fieldValue));
                reversestartoffset += Marshal.SizeOf(fieldValue);
            }
            else
            {
                var elementsize = Marshal.SizeOf(((Array)fieldValue).GetValue(0));

                reversestartoffset += ((Array)fieldValue).Length * elementsize;
            }
        }

        try
        {
            // copy byte array to ptr
            Marshal.Copy(temparray, startoffset, i, len);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ByteArrayToStructure FAIL" + ex.ToString());
        }

        obj = Marshal.PtrToStructure(i, obj.GetType());

        Marshal.FreeHGlobal(i);
    }

    /// <summary>
    /// Convert a struct to an array of bytes, struct fields being represented in 
    /// little endian (LSB first)
    /// </summary>
    /// <remarks>Note - assumes little endian host order</remarks>
    public static byte[] StructureToByteArray(object obj)
    {
        try
        {
            // fix's byte arrays that are too short or too long
            obj.GetType().GetFields()
                .Where(a => a.FieldType.IsArray && a.FieldType.UnderlyingSystemType == typeof(byte[]))
                .Where(a =>
                {
                    var attributes = a.GetCustomAttributes(typeof(MarshalAsAttribute), false);
                    if (attributes.Length > 0)
                    {
                        var marshal = (MarshalAsAttribute)attributes[0];
                        var sizeConst = marshal.SizeConst;
                        var data = (byte[])a.GetValue(obj);
                        if (data == null)
                        {
                            data = new byte[sizeConst];
                        }
                        else if (data.Length != sizeConst)
                        {
                            Array.Resize(ref data, sizeConst);
                            a.SetValue(obj, data);
                        }
                    }

                    return false;
                }).ToList();
        }
        catch
        {
        }

        var len = Marshal.SizeOf(obj);
        var arr = new byte[len];
        var ptr = Marshal.AllocHGlobal(len);
        Marshal.StructureToPtr(obj, ptr, true);
        Marshal.Copy(ptr, arr, 0, len);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    /// <summary>
    /// Convert a struct to an array of bytes, struct fields being represented in 
    /// big endian (MSB first)
    /// </summary>
    public static byte[] StructureToByteArrayBigEndian(params object[] list)
    {
        // The copy is made because SetValue won't work on a struct.
        // Boxing was used because SetValue works on classes/objects.
        // Unfortunately, it results in 2 copy operations.
        var thisBoxed = list[0]; // Why make a copy?
        var test = thisBoxed.GetType();

        var offset = 0;
        var data = new byte[Marshal.SizeOf(thisBoxed)];

        object fieldValue;
        TypeCode typeCode;

        byte[] temp;

        // Enumerate each structure field using reflection.
        foreach (var field in test.GetFields())
        {
            // field.Name has the field's name.

            fieldValue = field.GetValue(thisBoxed); // Get value

            // Get the TypeCode enumeration. Multiple types get mapped to a common typecode.
            typeCode = Type.GetTypeCode(fieldValue.GetType());

            switch (typeCode)
            {
                case TypeCode.Single: // float
                {
                    temp = BitConverter.GetBytes((float)fieldValue);
                    Array.Reverse(temp);
                    Array.Copy(temp, 0, data, offset, sizeof(float));
                    break;
                }
                case TypeCode.Int32:
                {
                    temp = BitConverter.GetBytes((int)fieldValue);
                    Array.Reverse(temp);
                    Array.Copy(temp, 0, data, offset, sizeof(int));
                    break;
                }
                case TypeCode.UInt32:
                {
                    temp = BitConverter.GetBytes((uint)fieldValue);
                    Array.Reverse(temp);
                    Array.Copy(temp, 0, data, offset, sizeof(uint));
                    break;
                }
                case TypeCode.Int16:
                {
                    temp = BitConverter.GetBytes((short)fieldValue);
                    Array.Reverse(temp);
                    Array.Copy(temp, 0, data, offset, sizeof(short));
                    break;
                }
                case TypeCode.UInt16:
                {
                    temp = BitConverter.GetBytes((ushort)fieldValue);
                    Array.Reverse(temp);
                    Array.Copy(temp, 0, data, offset, sizeof(ushort));
                    break;
                }
                case TypeCode.Int64:
                {
                    temp = BitConverter.GetBytes((long)fieldValue);
                    Array.Reverse(temp);
                    Array.Copy(temp, 0, data, offset, sizeof(long));
                    break;
                }
                case TypeCode.UInt64:
                {
                    temp = BitConverter.GetBytes((ulong)fieldValue);
                    Array.Reverse(temp);
                    Array.Copy(temp, 0, data, offset, sizeof(ulong));
                    break;
                }
                case TypeCode.Double:
                {
                    temp = BitConverter.GetBytes((double)fieldValue);
                    Array.Reverse(temp);
                    Array.Copy(temp, 0, data, offset, sizeof(double));
                    break;
                }
                case TypeCode.Byte:
                {
                    data[offset] = (byte)fieldValue;
                    break;
                }
                default:
                {
                    //System.Diagnostics.Debug.Fail("No conversion provided for this type : " + typeCode.ToString());
                    break;
                }
            }

            ; // switch
            if (typeCode == TypeCode.Object)
            {
                var length = ((byte[])fieldValue).Length;
                Array.Copy((byte[])fieldValue, 0, data, offset, length);
                offset += length;
            }
            else
            {
                offset += Marshal.SizeOf(fieldValue);
            }
        } // foreach

        return data;
    } // Swap

    public static MAVLink.message_info GetMessageInfo(this MAVLink.message_info[] source, uint msgid)
    {
        foreach (var item in source)
            if (item.msgid == msgid)
                return item;

        Console.WriteLine("Unknown Packet " + msgid);
        return new MAVLink.message_info();
    }
}