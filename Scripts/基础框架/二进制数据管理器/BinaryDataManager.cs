using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BinaryDataManager 
{
    private static BinaryDataManager _instance = new BinaryDataManager();
    public static BinaryDataManager Instance => _instance;
    
    private BinaryDataManager(){ }
    
    
    /// <summary>
    /// 存储读取二进制数据文件位置
    /// </summary>
    private static string DATA_PATH = Application.persistentDataPath;
    
    /// <summary>
    /// 数据存储至文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="data">预存入数据</param>
    public void SaveDataToFile(string path, string fileName, object data)
    {
        if(!Directory.Exists(DATA_PATH + path)) Directory.CreateDirectory(DATA_PATH + path);

        using (FileStream fs = new FileStream(DATA_PATH + path + fileName + ".BD", FileMode.Create, FileAccess.Write)) 
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, data);
            
            fs.Close();
        }
    }

    /// <summary>
    /// 从文件读取数据
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <typeparam name="T">存入数据类型</typeparam>
    /// <returns></returns>
    public T LoadDataFromFile<T>(string path, string fileName)
    {
        if(!File.Exists(DATA_PATH + path + fileName + ".BD")) return default(T);

        T data;
        using (FileStream fs = new FileStream(DATA_PATH + path + fileName + ".BD", FileMode.Open, FileAccess.Read))
        {
            BinaryFormatter bf = new BinaryFormatter();
            data = ((T)bf.Deserialize(fs));
            fs.Close();
        }
        
        return data;
    }

    /// <summary>
    /// 获取文件是否存在
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public bool GetFileExit(string fileName)
    {
        return File.Exists(DATA_PATH + fileName + ".BD");
    }
}