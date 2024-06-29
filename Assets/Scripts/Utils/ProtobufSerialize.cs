using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/*
 * protobuf的序列化与反序列化规则：
	1. 自定义类型规则：
		(1)[ProtoContract]特性：标识类可被序列化
		(2)[ProtoMember(id)]特性：标识属性可被序列化，id为数据索引，字段名修改不影响原数据
		(3)必须包含无参构造函数
	
 		实例：
			[ProtoContract]	
 			public class FData 
			{
				[ProtoMember(1)]	
				public int ID;
				[ProtoMember(2)]
				public string Name;
				public FData() {}
				public FData(int id, string name) 
				{
					ID = id;
					Name = name;
				}
			}	
	2.使用.proto文件作为配置表数据
		(1)通过protoc工具转换为cs文件,会自动添加特性
		(2)再序列化/反序列化cs对象
	3.继承问题
 */
namespace ProtobufSerialize
{

    public class ProtobufSerializer
    {

        /// <summary>
        /// 将消息序列化为二进制
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T obj)
        {

            try
            {
                //涉及格式转换，需要用到流，将二进制序列化到流中  
                using (MemoryStream ms = new MemoryStream())
                {
                    //使用ProtoBuf工具的序列化方法  
                    Serializer.Serialize(ms, obj);
                    //定义二级制数组，保存序列化后的结果  
                    byte[] result = new byte[ms.Length];
                    //将流的位置设为0，起始点  
                    ms.Position = 0;
                    //将流中的内容读取到二进制数组中  
                    ms.Read(result, 0, result.Length);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 反序列化消息
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="bytes">二进制消息体</param>
        /// <returns>消息体</returns>
        public static T DeSerialize<T>(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                //将消息写入流中  
                ms.Write(bytes, 0, bytes.Length);
                //将流的位置归0  
                ms.Position = 0;
                //使用工具反序列化对象  
                T result = default(T);
                result = Serializer.Deserialize<T>(ms);
                return result;
            }
        }

        public static void SaveByteFile(byte[] data, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                try
                {
                    fs.Write(data, 0, data.Length);
                    fs.Dispose();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static byte[] ReadByteByFile(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    byte[] buffur = new byte[fs.Length];
                    fs.Read(buffur, 0, (int)fs.Length);
                    fs.Dispose();
                    fs.Close();
                    return buffur;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}