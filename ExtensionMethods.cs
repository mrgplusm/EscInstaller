using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Common;
using Common.Commodules;
using EscInstaller.ViewModel.Settings;

namespace EscInstaller
{
    public static class ExtensionMethods
    {

        public static string GetHash<T>(this object instance) where T : HashAlgorithm, new()
        {
            var cryptoServiceProvider = new T();
            return ComputeHash(instance, cryptoServiceProvider);
        }

        /// <summary>
        ///     Gets a key based hash of the current instance.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the Cryptographic Service Provider to use.
        /// </typeparam>
        /// <param name="instance">
        ///     The instance being extended.
        /// </param>
        /// <param name="key">
        ///     The key passed into the Cryptographic Service Provider algorithm.
        /// </param>
        /// <returns>
        ///     A base 64 encoded string representation of the hash.
        /// </returns>
        public static string GetKeyedHash<T>(this object instance, byte[] key) where T : KeyedHashAlgorithm, new()
        {
            var cryptoServiceProvider = new T {Key = key};
            return ComputeHash(instance, cryptoServiceProvider);
        }

        /// <summary>
        ///     Gets a MD5 hash of the current instance.
        /// </summary>
        /// <param name="instance">
        ///     The instance being extended.
        /// </param>
        /// <returns>
        ///     A base 64 encoded string representation of the hash.
        /// </returns>
        public static string GetMD5Hash(this object instance)
        {
            return instance.GetHash<MD5CryptoServiceProvider>();
        }

        /// <summary>
        ///     Gets a SHA1 hash of the current instance.
        /// </summary>
        /// <param name="instance">
        ///     The instance being extended.
        /// </param>
        /// <returns>
        ///     A base 64 encoded string representation of the hash.
        /// </returns>
        public static string GetSHA1Hash(this object instance)
        {
            return instance.GetHash<SHA1CryptoServiceProvider>();
        }

        private static string ComputeHash<T>(object instance, T cryptoServiceProvider) where T : HashAlgorithm, new()
        {
            var serializer = new DataContractSerializer(instance.GetType());
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, instance);
                cryptoServiceProvider.ComputeHash(memoryStream.ToArray());
                return Convert.ToBase64String(cryptoServiceProvider.Hash);
            }
        }

        public static void ActivateNormalState(this Window window)
        {
            window.Activate();
            window.WindowState = WindowState.Normal;
        }

        private static readonly Random Random = new Random((int) DateTime.Now.Ticks); //thanks to McAden

        public static string RandomString(this int size)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26*Random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> collection) where T : class
        {
            var t = new SortedSet<T>();
            foreach (var element in collection)
            {
                t.Add(element);
            }
            return t;
        }

        public static void BubbleSort<T>(this IList<T> o) where T : IComparable<T>
        {
            for (var i = o.Count - 1; i >= 0; i--)
            {
                for (var j = 1; j <= i; j++)
                {
                    var o1 = o[j - 1];
                    var o2 = o[j];
                    if (o1.CompareTo(o2) <= 0) continue;
                    o.Remove(o1);
                    o.Insert(j, o1);
                }
            }
        }

        /// <summary>
        ///   Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
        /// 
        ///   Provides a method for performing a deep copy of an object.
        ///   Binary Serialization is used to perform the copy.
        /// </summary>
        /// <summary>
        ///   Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T"> The type of object being copied. </typeparam>
        /// <param name="source"> The object instance to copy. </param>
        /// <returns> The copied object. </returns>
        //public static T Clone<T>(this T source)
        //{
        //    if (!typeof(T).IsSerializable)
        //    {
        //        throw new ArgumentException("The type must be serializable.", "source");
        //    }

        //    // Don't serialize a null object, simply return the default for that object
        //    if (ReferenceEquals(source, null))
        //    {
        //        return default(T);
        //    }

        //    IFormatter formatter = new BinaryFormatter();
        //    Stream stream = new MemoryStream();
        //    using (stream)
        //    {
        //        formatter.Serialize(stream, source);
        //        stream.Seek(0, SeekOrigin.Begin);
        //        return (T)formatter.Deserialize(stream);
        //    }
        //}


         
  

        /// <summary>
        ///   garbage
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <returns> </returns>
        public static IEnumerable<T> EnumToList<T>()
        {
            var enumType = typeof (T);

            // Can't use generic type constraints on value types,   
            // so have to do check like this
            if (enumType.BaseType != typeof (Enum))
                throw new ArgumentException("T must be of type System.Enum");

            var enumValArray = Enum.GetValues(enumType);
            var enumValList = new List<T>(enumValArray.Length);

            enumValList.AddRange(from int val in enumValArray
                select (T) Enum.Parse(enumType, val.ToString(CultureInfo.InvariantCulture)));

            return enumValList;
        }

        public static MemberInfo GetMemberInfo(this System.Linq.Expressions.Expression expression)
        {
            MemberExpression operand;
            var lambdaExpression = (LambdaExpression) expression;
            if (lambdaExpression.Body as UnaryExpression != null)
            {
                var body = (UnaryExpression) lambdaExpression.Body;
                operand = (MemberExpression) body.Operand;
            }
            else
            {
                operand = (MemberExpression) lambdaExpression.Body;
            }
            return operand.Member;
        }


        /// <summary>
        ///   garbage
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public static string GetEnumDescription(Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof (DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static T XamlClone<T>(this T original) where T : class
        {
            if (original == null)
                return null;

            object clone;
            using (var stream = new MemoryStream())
            {
                XamlWriter.Save(original, stream);
                stream.Seek(0, SeekOrigin.Begin);
                clone = XamlReader.Load(stream);
            }

            if (clone is T)
                return (T) clone;
            else
                return null;
        }

        public static T HashPop<T>(this HashSet<T> hashSet)
        {
            var q = hashSet.First();
            hashSet.Remove(q);
            return q;
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetShortPathName(String pathName, StringBuilder shortName, int cbShortName);


        public static ObservableCollection<T> RemoveAll<T>(
            this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToArray();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return coll;
        }

    
    
    }
}
