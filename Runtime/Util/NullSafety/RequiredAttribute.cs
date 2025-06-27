using System;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MAVLinkAPI.Util.NullSafety
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    // [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)] TODO: enable this
    public class RequiredAttribute : PropertyAttribute
    {
    }

    // Add extension method for IsUnityNull
    public static class UnityObjectExtensions
    {
        public static bool IsUnityNull(this object obj)
        {
            if (obj == null)
                return true;

            // Check if it's a UnityEngine.Object and if it has been destroyed
            if (obj is Object unityObject)
                return unityObject == null;

            return false;
        }
    }

    public static class UnityRequiredChecker
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CheckField()
        {
            var gameObjs = Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var gameObj in gameObjs)
            {
                var type = gameObj.GetType();
                var fields = type.GetFields(BindingFlags.Instance |
                                            BindingFlags.Public |
                                            BindingFlags.NonPublic);

                var properties = type.GetProperties(BindingFlags.Instance |
                                                    BindingFlags.Public |
                                                    BindingFlags.NonPublic);

                var enabledOnClass = Attribute.GetCustomAttribute(type, typeof(RequiredAttribute)) is
                    RequiredAttribute;

                foreach (var field in fields)
                    if (enabledOnClass || Attribute.GetCustomAttribute(field, typeof(RequiredAttribute)) is
                            RequiredAttribute)
                    {
                        var value = field.GetValue(gameObj);
                        Report(value, type, field, gameObj);
                    }

                foreach (var property in properties)
                    if (enabledOnClass || Attribute.GetCustomAttribute(property, typeof(RequiredAttribute)) is
                            RequiredAttribute)
                        if (property.IsAccessor())
                        {
                            var value = property.GetValue(gameObj);
                            Report(value, type, property, gameObj);
                        }
            }

            void Report(object value, Type type, MemberInfo field, Object gameObject)
            {
                if (value == null || value.IsUnityNull())
                    Debug.LogException(
                        new NullReferenceException(
                            $"NullSafe violation: {type.Name}.{field.Name} is null"),
                        gameObject
                    );
            }
        }
    }
}