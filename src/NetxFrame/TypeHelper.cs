using System;
using System.Collections.Generic;
using System.Text;

namespace Netx
{
    public static  class TypeHelper
    {
        /// <summary>
        /// 检测是否是此类型或者父类是否是此类型
        /// </summary>
        /// <param name="type">当前类型</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>结果</returns>
        public static bool IsTypeOfBaseTypeIs(Type type, Type targetType)
        {
            if (type == targetType)
                return true;

            if (type.BaseType == null)
                return false;

            if (type.BaseType == targetType)
                return true;
            else
                return IsTypeOfBaseTypeIs(type.BaseType, targetType);
        }
    }
}
