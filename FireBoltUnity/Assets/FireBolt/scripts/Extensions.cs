using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LN.Utilities;
using System.Text.RegularExpressions;

namespace Assets.scripts
{
    public static class Extensions
    {
        private static readonly string doublePattern = @"[-+]?([0-9]+,)*[0-9]+(\.[0-9]*)?(E[-+][0-9]+)?";
        private static readonly Regex regex = new Regex(string.Format(@"^\s*(\(\s*(?<x>({0}))\s*,\s*(?<y>{0})\s*,\s*(?<z>{0})\s*\))\s*$", doublePattern), RegexOptions.ExplicitCapture);

        /// <summary>
        /// converts comma delimited string into a vector 3
        /// </summary>
        /// <param name="s">this better have x,y,z in it</param>
        /// <returns>shiny vector3</returns>
        public static bool TryParseVector3(this string s, out Vector3 v)
        {
            v = Vector3.zero;
            var match = regex.Match(s);
            if (match.Success)
            {
                v = new Vector3(float.Parse(match.Groups["x"].Value), float.Parse(match.Groups["y"].Value), float.Parse(match.Groups["z"].Value));
                return true;
            }
            return false;            
        }


        /// <summary>
        /// converts comma delimited numeric pair into x,z coordinates
        /// </summary>
        /// <param name="s">string of format x,z </param>
        /// <returns>vector 3</returns>     
        public static bool TryParsePlanarCoords(this string s, out Vector2 v)
        {
            v = Vector3.zero;
            string[] values = s.Split(new char[] { ',' });
            float x, z;
            if (values.Length > 1 &&
                float.TryParse(values[0], out x )&&
                float.TryParse(values[1], out z)) //we got two coords
            {
                v = new Vector2((float)x,(float)z);
                return true;
            }
            return false;
        }

        /// <summary>
        /// gives a pretty cyan box at the bounds...only in scene view as it's a debug thinger
        /// </summary>
        /// <param name="bounds"></param>
        public static void BuildDebugBox(this Bounds bounds)
        {
            //add some debugging box for the area we think we are framing
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
            Vector3[] corners = new Vector3[8];
            //top face
            corners[0] = new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
            corners[1] = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);
            corners[2] = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
            corners[3] = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
            //bottom face
            corners[4] = new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z);
            corners[5] = new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z);
            corners[6] = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
            corners[7] = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
            //gogo complete graph!
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (i == j) continue;
                    Debug.DrawLine(corners[i], corners[j], Color.cyan, 150);
                }
            }
        }

        /// <summary>
        /// finds the angle of rotation to change orientation from "from" to "to"
        /// ignores Y values
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>degree measure of needed rotation -180 to 180</returns>
        public static float GetXZAngleTo(this Vector3 from, Vector3 to)
        {

            Vector2 from2d = new Vector2(from.x, from.z);
            Vector2 to2d = new Vector2(to.x, to.z);
            Vector2 direction = to2d - from2d;
            float radians = Mathf.Atan2(direction.x,direction.y);
            if (radians > Mathf.PI) radians -= 2 * Mathf.PI;
            else if (radians < -Mathf.PI) radians += 2 * Mathf.PI;
            return radians * 180/Mathf.PI;
        }

        public static Vector3 ToVector3(this Impulse.v_1_336.Constants.Coordinate2D from)
        {
            return new Vector3((float)from.X, 0, (float)from.Y);
        }

        public static Vector3 ToVector3(this Impulse.v_1_336.Constants.Coordinate2D from, float domainToEngine)
        {
            return new Vector3((float)from.X, 0, (float)from.Y)*1/domainToEngine;
        }

        public static Vector3Nullable ToVector3Nullable(this Impulse.v_1_336.Constants.Coordinate2D from, float domainToEngine)
        {
            return new Vector3Nullable((float)from.X * 1 / domainToEngine, null, (float)from.Y * 1 / domainToEngine);
        }

        public static Vector3Nullable ToVector3Nullable(this Impulse.v_1_336.Constants.Coordinate3D from, float domainToEngine)
        {
            return new Vector3Nullable((float)from.X * 1 / domainToEngine, (float)from.Y * 1 / domainToEngine, (float)from.Z * 1 / domainToEngine);
        }

        public static Vector3 ToVector3(this Impulse.v_1_336.Constants.Coordinate3D from, float domainToEngine)
        {
            return new Vector3((float)from.X, (float)from.Y, (float)from.Z) * 1 / domainToEngine;
        }

        public static float ToMillis(this uint tick, uint millisPerTick)
        {
            return tick * millisPerTick;
        }

        /// <summary>
        /// applies the specified values in this Vector3Nullable, newValues, over those in overridden.
        /// for all unspecified values in newValues, overridden controls.
        /// </summary>
        /// <param name="newValues"></param>
        /// <param name="overridden"></param>
        /// <returns></returns>
        public static Vector3 Merge(this Vector3Nullable newValues, Vector3 overridden)
        {
            return new Vector3(newValues.X.HasValue ? newValues.X.Value : overridden.x,
                               newValues.Y.HasValue ? newValues.Y.Value : overridden.y,
                               newValues.Z.HasValue ? newValues.Z.Value : overridden.z);
        }

        public static float convertSourceEngineToUnityRotation(this float sourceDegrees)
        {
            float unityDegrees = -sourceDegrees + 90;
            unityDegrees = unityDegrees.BindToSemiCircle();
            //% 360;
            //while (unityDegrees > 180)
            //{
            //    unityDegrees -= 360;
            //}
            //while (unityDegrees < -180)
            //{
            //    unityDegrees += 360;
            //}
            return unityDegrees;
        }

        public static float BindToSemiCircle(this float theta)
        {
            theta = theta % 360;
            while (theta > 180)
            {
                theta -= 360;
            }
            while (theta < -180)
            {
                theta += 360;
            }
            return theta;
        }

        public static string AppendTimestamps(this string s)
        {
            return s + string.Format(" d:s[{0}:{1}]", ElPresidente.currentDiscourseTime, ElPresidente.currentStoryTime);
        }
    }
}
