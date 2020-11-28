using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using WolvenKit.CR2W.Reflection;
using static WolvenKit.CR2W.Types.Enums;
using FastMember;

namespace WolvenKit.CR2W.Types
{
    public partial class CFXTrackItem : CFXBase
    {

        [Ordinal(1000)] [REDBuffer(true)] public CName buffername { get; set; }
        [Ordinal(1001)] [REDBuffer(true)] public CDynamicInt count { get; set; }
        [Ordinal(1002)] [REDBuffer(true)] public CUInt8 unk { get; set; }
        [Ordinal(1003)] [REDBuffer(true)] public CCompressedBuffer<CBufferUInt16<CFloat>> buffer { get; set; }

        public CFXTrackItem(CR2WFile cr2w, CVariable parent, string name) : base(cr2w, parent, name)
        {
            buffername = new CName(cr2w, this, nameof(buffername)) { IsSerialized = true };
            count = new CDynamicInt(cr2w, this, nameof(count)) { IsSerialized = true };
            unk = new CUInt8(cr2w, this, nameof(unk)) { IsSerialized = true };
            buffer = new CCompressedBuffer<CBufferUInt16<CFloat>>(cr2w, this, nameof(buffer)) { IsSerialized = true };

        }


        public override void Read(BinaryReader file, uint size)
        {
            var startpos = file.BaseStream.Position;
            base.Read(file, size);

            var endpos = file.BaseStream.Position;
            var bytesread = endpos - startpos;
            var bytesleft = size - bytesread;

            if (bytesread < size)
            {
                var startpos2 = file.BaseStream.Position;

                buffername.Read(file, 2);
                count.Read(file, size);
                unk.Read(file, 1);
                buffer.Read(file, 0, count.val);

                var endpos2 = file.BaseStream.Position;
                var bytesread2 = endpos2 - startpos2;
                var bytesleft2 = bytesleft - bytesread2;
            }
            else if (bytesread > size)
            {
                // read too far: ERROR
                throw new InvalidParsingException("Bytes read too far");
            }
            else
            {
                // no additional bytes left.
                // investigate if there is a reason when that happens
            }   
        }

        public override void Write(BinaryWriter file)
        {
            base.Write(file);

            // if buffername value is not null, then something was read
            // or the user edited the buffer name
            // in any case, write all of the additional data
            if (buffername.REDValue != null)
            {
                buffername.Write(file);
                count.Write(file);
                unk.Write(file);
                buffer.Write(file);
            }
            // if that is not the case then the additional data was not read on start
            // but it could be that the user edited one of the other values 
            // so we need to check them. if any of those is greater than 0 (they are always not null bc of the constructor)
            // then we write the whole buffer
            else
            {
                if (count.val > 0
                    || unk.val > 0
                    || buffer.Count > 0)
                {
                    buffername.Write(file);
                    count.Write(file);
                    unk.Write(file);
                    buffer.Write(file);
                }
            }
        }


    }
}