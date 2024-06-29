using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/*
 * protobuf�����л��뷴���л�����
	1. �Զ������͹���
		(1)[ProtoContract]���ԣ���ʶ��ɱ����л�
		(2)[ProtoMember(id)]���ԣ���ʶ���Կɱ����л���idΪ�����������ֶ����޸Ĳ�Ӱ��ԭ����
		(3)��������޲ι��캯��
	
 		ʵ����
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
	2.ʹ��.proto�ļ���Ϊ���ñ�����
		(1)ͨ��protoc����ת��Ϊcs�ļ�,���Զ��������
		(2)�����л�/�����л�cs����
	3.�̳�����
 */
namespace ProtobufSerialize
{

    public class ProtobufSerializer
    {

        /// <summary>
        /// ����Ϣ���л�Ϊ������
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T obj)
        {

            try
            {
                //�漰��ʽת������Ҫ�õ����������������л�������  
                using (MemoryStream ms = new MemoryStream())
                {
                    //ʹ��ProtoBuf���ߵ����л�����  
                    Serializer.Serialize(ms, obj);
                    //������������飬�������л���Ľ��  
                    byte[] result = new byte[ms.Length];
                    //������λ����Ϊ0����ʼ��  
                    ms.Position = 0;
                    //�����е����ݶ�ȡ��������������  
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
        /// �����л���Ϣ
        /// </summary>
        /// <typeparam name="T">��Ϣ����</typeparam>
        /// <param name="bytes">��������Ϣ��</param>
        /// <returns>��Ϣ��</returns>
        public static T DeSerialize<T>(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                //����Ϣд������  
                ms.Write(bytes, 0, bytes.Length);
                //������λ�ù�0  
                ms.Position = 0;
                //ʹ�ù��߷����л�����  
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