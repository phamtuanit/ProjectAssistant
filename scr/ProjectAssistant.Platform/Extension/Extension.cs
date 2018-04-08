namespace ProjectAssistant.Platform.Extension
{
    using System.Collections.Generic;

    public static class Extension
    {
        /// <summary>
        /// Determines whether [is null or empty] [the specified collection].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns><c>true</c> if [is null or empty] [the specified collection]; otherwise, <c>false</c>.</returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
    }
}
