using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Mathematics;

namespace ECS_MLAgents_v0.Data
{
    public enum SensorDataType : int {
        FLOAT,
        // INT,
        // BOOL,
        // ENUM,
        // TEXTURE,

    } 

    public enum SensorType : int {
        DATA,
        REWARD,
        DONE,
        ID,

    } 

    public class SensorAttribute : Attribute{ 
        public string Description;
        public SensorType SensorType;
        public SensorAttribute(SensorType Type, string Description = ""){
            this.Description = Description;
            this.SensorType = Type;
        }

    }
    


    public class AttributeUtility
    {
        // Note : This is non blittable
        public struct SensorMetadata
        {
            public char64 Name;
            public char256 Description;
            // public int4 Dimension;
            // public int Offset;
            // public SensorDataType DataType;
            public SensorType SensorType;
        }
        
        public static SensorMetadata[] GetSensorMetaData(Type t)
        {
            var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            // var fields = t.GetFields();
            var result = new SensorMetadata[fields.Length];
            for(var i =0; i<fields.Length; i++)
            {
                result[i] = GetMetaDataFromField(fields[i]);
            }
            return result;
        }

        public static SensorMetadata GetMetaDataFromField(FieldInfo field)
        {
            //TODO : Run some checks on the field to make sure the data is right


            var attribute = field.GetCustomAttribute<SensorAttribute>();
            if (attribute == null){
                attribute = new SensorAttribute(SensorType.DATA);
            }

            return new SensorMetadata{
                Name = new char64(field.Name),
                SensorType = attribute.SensorType,
                Description = new char256(attribute.Description),
            };
        }

        
        
    }
}
