//using com.itac.model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.itac.action
{
    public static class JsonHelper
    {
        public static Dictionary<string, string> ReadJsonValue(string jsonText)
        {
            try
            {
                Dictionary<string, string> dicJsonValue = new Dictionary<string, string>();
                JObject jo = JObject.Parse(jsonText);
                string[] keys = jo.Properties().Select(item => item.Name.ToString()).ToArray();
                if (keys != null && keys.Length > 0)
                {
                    foreach (var itemKey in keys)
                    {
                        string value = jo.Property(itemKey).Value.ToString();
                        //if (VerifyJsonFormate(value))
                        //{
                        //    Dictionary<string, string> dicJsonValueSub = ReadJsonValue(value);
                        //    if (dicJsonValueSub != null && dicJsonValueSub.Count > 0)
                        //    {
                        //        foreach (var item in dicJsonValueSub.Keys)
                        //        {
                        //            dicJsonValue[item] = dicJsonValueSub[item];
                        //        }
                        //    }
                        //}
                        //else if (VerifyJsonArrayFormate(value))
                        //{
                        //    ReadJsonValueWithArray(value);
                        //}
                        //else
                        dicJsonValue[itemKey] = value;
                    }
                }

                return dicJsonValue;
            }
            catch (Exception ex)
            {
                //LogHelper.Error(ex);
                return null;
            }
        }

        public static string ReadJsonValue(string jsonkey, string jsonText)
        {
            try
            {
                string jsonValue = "";
                JObject jo = JObject.Parse(jsonText);
                jsonValue = jo.Property(jsonkey).Value.ToString();
                return jsonValue;
            }
            catch (Exception ex)
            {
                //LogHelper.Error(ex);
                return null;
            }
        }

        public static string ReadJsonValueWithArray(string jsonText)
        {
            try
            {
                string jsonValue = "";
                JArray jar = JArray.Parse(jsonText);
                return jsonValue;
            }
            catch (Exception ex)
            {
                //LogHelper.Error(ex);
                return null;
            }
        }

        public static JArray ReadJsonValueWithArrayExt(string jsonText)
        {
            try
            {
                JArray jar = JArray.Parse(jsonText);
                return jar;
            }
            catch (Exception ex)
            {
                //    LogHelper.Error(ex);
                return null;
            }
        }

        //public static bool VerifyJsonFormate(string jsonText)
        //{
        //    try
        //    {
        //        JObject jo = JObject.Parse(jsonText);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Error(ex);
        //        return false;
        //    }
        //}

        //public static bool VerifyJsonArrayFormate(string jsonText)
        //{
        //    try
        //    {
        //        JArray jr = JArray.Parse(jsonText);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        //LogHelper.Error(ex);
        //        return false;
        //    }
        //}

        //使用LINQ to JSON前，需要引用Newtonsoft.Json的dll和using Newtonsoft.Json.Linq的命名空间
        //LINQ to JSON主要使用到JObject, JArray, JProperty和JValue这四个对象
        //JObject用来生成一个JSON对象，简单来说就是生成”{}”
        //JArray用来生成一个JSON数 组，也就是”[]”
        //JProperty用来生成一个JSON数据，格式为key/value的值
        //JValue则直接生成一个JSON值
        //public static string GenerateJSONString()
        //{
        //    return new JObject(
        //                 new JProperty("total", 5),
        //                 new JProperty("rows",
        //                         new JArray(
        //                                  new JObject(
        //                                               new JProperty("studentID", "123"),
        //                                               new JProperty("name", "zhang san"),
        //                                               new JProperty("homeTown", "su zhou")
        //                                      )
        //                             )
        //                     )
        //             ).ToString();
        //}

        //public static string GenerateJSONString(List<JsonItemEntity> jsonValues)
        //{
        //    StringWriter sw = new StringWriter();
        //    JsonWriter writer = new JsonTextWriter(sw);
        //    writer.WriteStartObject();
        //    if (jsonValues.Count > 0)
        //    {
        //        foreach (var item in jsonValues)
        //        {
        //            writer.WritePropertyName(item.Name);
        //            writer.WriteValue(item.Value);
        //        }
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //    writer.WriteEndObject();
        //    writer.Flush();
        //    string jsonText = sw.GetStringBuilder().ToString();
        //    return jsonText;
        //}

        //public static string GenerateJSONString(List<JsonItemEntity> jsonValues, List<List<JsonItemEntity>> arrayList, string arrayName)
        //{
        //    StringWriter sw = new StringWriter();
        //    JsonWriter writer = new JsonTextWriter(sw);
        //    writer.WriteStartObject();
        //    if (jsonValues.Count > 0)
        //    {
        //        foreach (var item in jsonValues)
        //        {
        //            writer.WritePropertyName(item.Name);
        //            writer.WriteValue(item.Value);
        //        }
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //    writer.WritePropertyName(arrayName);
        //    writer.WriteStartArray();
        //    if (arrayList != null && arrayList.Count > 0)
        //    {
        //        foreach (var itemArray in arrayList)
        //        {
        //            writer.WriteStartObject();
        //            foreach (var itemUnit in itemArray)
        //            {
        //                writer.WritePropertyName(itemUnit.Name);
        //                writer.WriteValue(itemUnit.Value);
        //            }
        //            writer.WriteEndObject();
        //        }
        //    }
        //    writer.WriteEndArray();
        //    writer.WriteEndObject();
        //    writer.Flush();
        //    string jsonText = sw.GetStringBuilder().ToString();
        //    return jsonText;
        //}
    }
}
