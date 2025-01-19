using System.Collections.Concurrent;
using System.Reflection;

namespace Celeste.Mod.LeaveTheCastle.Utils;

// copy from Celeste TAS

internal static class ReflectionExtensions {
    private const BindingFlags StaticInstanceAnyVisibility =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private const BindingFlags InstanceAnyVisibilityDeclaredOnly =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private record struct MemberKey(Type Type, string Name) {
        public readonly Type Type = Type;
        public readonly string Name = Name;
    }

    private static readonly ConcurrentDictionary<MemberKey, FieldInfo> CachedFieldInfos = new();

    public static FieldInfo GetFieldInfo(this Type type, string name) {
        var key = new MemberKey(type, name);
        if (CachedFieldInfos.TryGetValue(key, out var result)) {
            return result;
        }

        do {
            result = type.GetField(name, StaticInstanceAnyVisibility);
        } while (result == null && (type = type.BaseType) != null);

        return CachedFieldInfos[key] = result;
    }



    public static object GetFieldValue(this object obj, string name) {
        return obj.GetType().GetFieldInfo(name)?.GetValue(obj);
    }

    public static T GetFieldValue<T>(this object obj, string name) {
        object result = obj.GetType().GetFieldInfo(name)?.GetValue(obj);
        if (result == null) {
            return default;
        }
        else {
            return (T)result;
        }
    }
}

internal static class HashCodeExtensions {
    public static long GetCustomHashCode<T>(this IEnumerable<T> enumerable) {
        if (enumerable == null) {
            return 0;
        }

        unchecked {
            long hash = 17;
            foreach (T item in enumerable) {
                hash = hash * -1521134295 + EqualityComparer<T>.Default.GetHashCode(item);
            }

            return hash;
        }
    }
}

internal static class TypeExtensions {

    public static bool IsConst(this FieldInfo fieldInfo) {
        return fieldInfo.IsLiteral && !fieldInfo.IsInitOnly;
    }
}


internal static class EnumerableExtensions {

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) {
        return enumerable == null || !enumerable.Any();
    }

}
