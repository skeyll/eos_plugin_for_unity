/*
 * Copyright (c) 2024 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace PlayEveryWare.EpicOnlineServices.Utility
{
    using System;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Utility class used for a variety of File tasks.
    /// </summary>
    public static class FileUtility
    {
        /// <summary>
        /// Generates a unique and new temporary directory inside the Temporary Cache Path as determined by Unity,
        /// and returns the fully-qualified path to the newly created directory.
        /// </summary>
        /// <returns>Fully-qualified file path to the newly generated directory.</returns>
        public static string GenerateTempDirectory()
        {
            // Generate a temporary directory path.
            string tempDirectory = Path.Combine(Application.temporaryCachePath, $"/Output-{Guid.NewGuid()}/");

            // If (by some crazy miracle) the directory path already exists, keep generating until there is a new one.
            while (Directory.Exists(tempDirectory))
            {
                tempDirectory = Path.Combine(Application.temporaryCachePath, $"/Output-{Guid.NewGuid()}/");
            }

            // Create the directory.
            Directory.CreateDirectory(tempDirectory);

            // return the fully-qualified path to the newly created directory.
            return Path.GetFullPath(tempDirectory);
        }

        /// <summary>
        /// Returns the root of the Unity project.
        /// </summary>
        /// <returns>Fully-qualified file path to the root of the Unity project.</returns>
        public static string GetProjectPath()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
        }
    }
}