using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HMFW.Tools
{
    public class Util
    {
        /// <summary>
        /// 获取某个基类的所有的非抽象子类
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static List<Type> GetAllSubClass(Type baseType)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> types = new List<Type>();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var subTypes = assemblies[i].GetTypes()
                    .Where(type => baseType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);
                types.AddRange(subTypes);
            }

            return types;
        }
    }
}