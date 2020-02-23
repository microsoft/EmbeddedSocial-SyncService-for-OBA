// <copyright file="StringToTableKeyTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.Utils;

    /// <summary>
    /// Tests the StringToTableKey method
    /// </summary>
    [TestClass]
    public class StringToTableKeyTests
    {
        /// <summary>
        /// Tests a plain string that should pass without any changes
        /// </summary>
        [TestMethod]
        public void PlainString()
        {
            string inputString = "Hello world";
            string outputString = inputString.StringToTableKey();
            Assert.AreEqual(inputString, outputString);
        }

        /// <summary>
        /// Tests a string with funny characters that should change
        /// </summary>
        [TestMethod]
        public void FunnyString()
        {
            string inputString = "Hello world \\ # % + / ? \u0082";
            string outputString = inputString.StringToTableKey();
            Assert.AreNotEqual(inputString, outputString);
            Assert.IsTrue(outputString.StartsWith("Hello world "));
            Assert.IsFalse(outputString.Contains("\\"));
            Assert.IsFalse(outputString.Contains("#"));
            Assert.IsFalse(outputString.Contains("%"));
            Assert.IsFalse(outputString.Contains("+"));
            Assert.IsFalse(outputString.Contains("/"));
            Assert.IsFalse(outputString.Contains("?"));
            Assert.IsFalse(outputString.Contains("\u0082"));
        }
    }
}