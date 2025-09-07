using mppc_cs;

FileStream fs = File.OpenRead("elements.data");
BinaryReader binaryReader = new(fs);

//short _version = binaryReader.ReadInt16();
//short _signature = binaryReader.ReadInt16();
//uint _timestamp = binaryReader.ReadUInt32();
//byte[] _orgData = binaryReader.ReadBytes(binaryReader.ReadInt32());
//uint _tagName = binaryReader.ReadUInt32();
//byte[] _computerName = binaryReader.ReadBytes(binaryReader.ReadInt32());
//uint _computerTimestamp = binaryReader.ReadUInt32();
//uint _tagInfo = binaryReader.ReadUInt32();
//byte[] _hardInfo = binaryReader.ReadBytes(binaryReader.ReadInt32());
_ = binaryReader.ReadInt16();
_ = binaryReader.ReadInt16();
_ = binaryReader.ReadUInt32();
_ = binaryReader.ReadBytes(binaryReader.ReadInt32());
_ = binaryReader.ReadUInt32();
_ = binaryReader.ReadBytes(binaryReader.ReadInt32());
_ = binaryReader.ReadUInt32();
_ = binaryReader.ReadUInt32();
_ = binaryReader.ReadBytes(binaryReader.ReadInt32());
//--------------------------------------------------------------------------------------
List<IDSize> list = [];
int count = binaryReader.ReadInt32();
for (int i = 0; i < count; i++)
{
    list.Add(new IDSize
    {
        id = binaryReader.ReadUInt32(),
        size = binaryReader.ReadUInt16(),
    });
}

_ = binaryReader.ReadInt32();

for (int i = 0; i < count; i++)
{
    byte[] buf = binaryReader.ReadBytes(list[i].size);//读取数据
    byte[] dec = MppcDecompress.Decompress(buf, 84);//解压数据
    byte[] enc = MppcCompress.Compress(dec);//压缩数据

    if (Convert.ToHexString(buf) != Convert.ToHexString(enc))
    {
        Console.WriteLine("出错了！！！！！！");
        break;
    }
    Console.WriteLine(Convert.ToHexString(buf));
    Console.WriteLine(Convert.ToHexString(enc));
    Console.WriteLine(Convert.ToHexString(dec));
    Console.WriteLine("-----------------------------------------------");
}

Console.ReadKey();
