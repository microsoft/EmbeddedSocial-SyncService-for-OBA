/* Copyright 2014 Michael Braude and individual contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace OBAService.OBAClient
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// A class full of helpful extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the first elements value as type T.
        /// </summary>
        /// <typeparam name="T">Type of the returned value</typeparam>
        /// <param name="element">XElement</param>
        /// <param name="childNodeName">ChildNode Name in XElement</param>
        /// <returns>First element's value of the specified child node</returns>
        public static T GetFirstElementValue<T>(this XElement element, string childNodeName)
        {
            var childNode = element.Descendants(childNodeName).FirstOrDefault();
            if (childNode == null)
            {
                return default(T);
            }

            try
            {
                return (T)Convert.ChangeType(childNode.Value.Trim(), typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
    }
}
