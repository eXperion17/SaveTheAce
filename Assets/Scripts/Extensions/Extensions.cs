using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Public static class for static method's that could be useful for the whole program.
/// </summary>
public static class Extensions
{
	public static bool IsMobileTarget
	{
		get
		{
#if UNITY_ANDROID || UNITY_IOS
			return true;
#else
			return false;
#endif
		}
	}

	private static int uniqueID = 0;
	public static int UniqueID {
		get {
			uniqueID++;
			return uniqueID-1;
		}
	}




	//Parsing (its a mess)
	public static int[] ParseStringIntoIntArray(string info, int size) {
		if (info.Length == 0)
			return new int[size];

		int[] list = new int[size];
		string[] conv = info.Split('_');

		for (int i = 0; i < conv.Length; i++) {
			list[i] = int.Parse(conv[i]);
		}

		return list;
	}

	public static int[,] ParseStringIntoDoubleIntArray(string info, int size) {
		if (info.Length == 0)
			return new int[size, size - 1];

		int[,] list = new int[size, size - 1];
		string[] conv = info.Split('_');

		for (int i = 0; i < conv.Length; i++) {
			if (conv[i].Length == 1 && int.Parse(conv[i]) == 0) {
				list[i, 0] = list[i, 1] = 0;
			} else {
				string[] dble = conv[i].Split('-');
				list[i, 0] = int.Parse(dble[0]);
				list[i, 1] = int.Parse(dble[1]);
			}
		}

		return list;
	}

	public static List<int> ParseStringIntoList(string hand) {
		if (hand.Length == 0)
			return new List<int>();

		List<int> list = new List<int>();
		string[] conv = hand.Split('_');
		for (int i = 0; i < conv.Length; i++) {
			list.Add(int.Parse(conv[i]));
		}

		return list;
	}








	// Variable used to cache our light object. Prevents frequent calls to GameObject.Find and similar heavy calls.
	public static readonly Vector3 Zero = Vector3.zero;


	public static bool IsCursorOverObject()
	{
		if (Application.isMobilePlatform)
		{
			return IsPointerOverUIObject();
		}

		// No event system at this time. Impossible to know if the cursor is over an object, assume false.
		if (UnityEngine.EventSystems.EventSystem.current == null)
			return false;

		return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    public static bool IsPointerOverUIObject()
    {
        UnityEngine.EventSystems.PointerEventData eventDataCurrentPosition = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    /// <summary>
    /// Gets all GameObjects currently over the pointer through a Raycast.
    /// </summary>
    /// <returns>All the GameObjects being targeted by the pointer, through all the layers.</returns>
    public static List<GameObject> GetGameObjectsOverPointer()
    {
        UnityEngine.EventSystems.PointerEventData eventDataCurrentPosition = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        List<GameObject> objects = new List<GameObject>();
        results.ForEach(x => objects.Add(x.gameObject));
        return objects;
    }
	

	public static void ChangeParentScale(this Transform parent, float scale)
	{
		List<Transform> children = new List<Transform>();
		foreach (Transform child in parent)
		{
			child.SetParent(null);
			children.Add(child);
		}
		parent.localScale = new Vector3(scale, scale, scale);
		foreach (Transform child in children) child.SetParent(parent);
	}

	public static float Truncate(this float value, int digits)
	{
		double mult = Math.Pow(10.0, digits);
		double result = Math.Truncate(mult * value) / mult;
		return (float)result;
	}

	public static int FindObjectIndexWithinParent(Transform parent, Transform child) {
		for (int i = 0; i < parent.childCount; i++) {
			var currChild = parent.GetChild(i);

			if (currChild.Equals(child)) {
				Debug.Log("aaa " + i);
				return i;
			}
		}

		//Debug.LogError("Couldn't find the child within the given parent!");
		return -1;
	}

	/// <summary>
	/// For randomly shuffling an array using System.Security.Cryptography, because it has better random functions than the standard ones.
	/// </summary>
	/// <typeparam name="T">List you want shuffled</typeparam>
	/// <param name="list">Same list, but all the elements in a random order.</param>
	public static void Shuffle<T>(this IList<T> list)
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static float ClampAngle(float angle, float min = -360f, float max = 360f)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360.0)
            angle -= 360f;

        return Mathf.Clamp(angle, min, max);
    }

    public static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

    public static bool IsNanSimplified(this Vector3 vector)
    {
        return (!float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z));
    }

    public static string SplitCamelCase(string source)
    {
        string[] split = Regex.Split(source, @"(?<!^)(?=[A-Z])");
        return String.Join(" ", split);
    }

    public static bool IsLayerInMask(int layer, LayerMask mask)
    {
        return mask == (mask | (1 << layer));
    }

    /// <summary>
    /// Shuffle the specified array using the Fisher Yates algorithm.
    /// </summary>
    /// <param name="array">Array</param>
    public static void Shuffle<T>(T[] array)
    {
        System.Random _random = new System.Random();
        if (array != null && array.Length != 0)
        {
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                // NextDouble returns a random number between 0 and 1.
                // ... It is equivalent to Math.random() in Java.
                int r = i + (int)(_random.NextDouble() * (n - i));
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }
    }

    /// <summary>
    /// Gets a part of an array structure.
    /// </summary>
    /// <typeparam name="T">The type the data will be given in.</typeparam>
    /// <param name="data">The data for which a part is needed.</param>
    /// <param name="index">The index of the first element from which the partition should start.</param>
    /// <param name="length">The length of the partition.</param>
    /// <returns>The part of the input structure.</returns>
    public static T[] SubArray<T>(this T[] data, int index, int length)
    {
        T[] result = new T[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }

    public static int Wrap(int index, int size)
    {
        return ((index % size) + size) % size;
    }

    public static Vector3 Lerp(Vector3 vFrom, Vector3 vTo, float percentage)
    {
        return (1 - percentage) * vFrom + percentage * vTo;
    }

    public static DateTime ConvertUnixTimeStamp(string unixTimeStamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(unixTimeStamp));
    }

    public static CultureInfo GetCurrentCultureInfo(SystemLanguage language)
    {
        return CultureInfo.GetCultures(CultureTypes.AllCultures).
            FirstOrDefault(x => x.EnglishName == Enum.GetName(language.GetType(), language));
    }

    public static CultureInfo GetCurrentCultureInfo()
    {
        SystemLanguage currentLanguage = Application.systemLanguage;
        CultureInfo correspondingCultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(x => x.EnglishName.Equals(currentLanguage.ToString()));
        return CultureInfo.CreateSpecificCulture(correspondingCultureInfo.TwoLetterISOLanguageName);
    }

    public static string UnescapeCodes(string src)
    {
        var rx = new Regex("\\\\([0-9A-Fa-f]+)");
        var res = new StringBuilder();
        var pos = 0;
        foreach (Match m in rx.Matches(src))
        {
            res.Append(src.Substring(pos, m.Index - pos));
            pos = m.Index + m.Length;
            res.Append((char)Convert.ToInt32(m.Groups[1].ToString(), 16));
        }
        res.Append(src.Substring(pos));
        return res.ToString();
    }

    public static string StripHTML(string input)
    {
        return Regex.Replace(input, "<.*?>", String.Empty);
    }

    public static IEnumerator FilterDoublesFromString(string original, string chars, string replaceWith, Action<string> result)
    {
        while (original.Contains(chars))
        {
            original = original.Replace(chars, replaceWith);
            yield return null;
        }

        result(original);

        yield return null;
    }

    public static bool IsNan(this float f)
    {
        return float.IsNaN(f);
    }

    public static bool IsNan(this Vector3 vector)
    {
        if (float.IsNaN(vector.x) || float.IsInfinity(vector.x))
            return true;
        if (float.IsNaN(vector.y) || float.IsInfinity(vector.y))
            return true;
        if (float.IsNaN(vector.z) || float.IsInfinity(vector.z))
            return true;

        return false;
    }

    public static bool IsNan(Quaternion q)
    {
        if (float.IsNaN(q.x) || float.IsInfinity(q.x))
            return true;
        if (float.IsNaN(q.y) || float.IsInfinity(q.y))
            return true;
        if (float.IsNaN(q.z) || float.IsInfinity(q.z))
            return true;
        if (float.IsNaN(q.w) || float.IsInfinity(q.w))
            return true;

        return false;
    }

    /// <summary>
    /// Used in Shuffle(T).
    /// </summary>
    private static System.Random _random = new System.Random();

    /// <summary>
    /// Shuffle the array.
    /// </summary>
    /// <typeparam name="T">Array element type.</typeparam>
    /// <param name="array">Array to shuffle.</param>
    static void ShuffleGeneric<T>(T[] array)
    {
        int n = array.Length;
        for (int i = 0; i < n; i++)
        {
            int r = i + (int)(_random.NextDouble() * (n - i));
            T t = array[r];
            array[r] = array[i];
            array[i] = t;
        }
    }

	public static T Next<T>(this T src) where T : struct
	{
		if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

		T[] Arr = (T[])Enum.GetValues(src.GetType());
		int j = Array.IndexOf<T>(Arr, src) + 1;
		return (Arr.Length==j) ? Arr[0] : Arr[j];
	}

	/// <summary>
	/// Flip a queue. Note that this may throw a stackoverflow error
	/// </summary>
	public static Queue<T> FlipQueue<T>(Queue<T> q)
    {
        Queue<T> returnQueue = new Queue<T>();
        RecursiveFlipQueue<T>(q, returnQueue);
        return returnQueue;
    }

    private static void RecursiveFlipQueue<T>(Queue<T> source, Queue<T> destination)
    {
        T buffer = source.Dequeue();
        if (source.Count != 0)
        {
            RecursiveFlipQueue(source, destination);
        }
        destination.Enqueue(buffer);
    }

    public static void FlipArray<T>(T[] source)
    {
        for (int i = 0; i < source.Length / 2; i++)
        {
            T temp = source[i];
            source[i] = source[source.Length - i - 1];
            source[source.Length - i - 1] = temp;
        }
    }

    public static bool Approximately(float f1, float f2, float diff)
    {
        return Mathf.Abs(f2 - f1) < diff;
    }

    public static float Round(float f, float digits)
    {
        float factor = Mathf.Pow(10, digits);
        return Mathf.Round(f * factor) / factor;
    }

    public static bool IsNullOrEmpty(string value)
    {
        return value == null || value == String.Empty;
    }

    /// <summary>
    /// Clamp a value, excluding the range between minNeg and minPos
    /// </summary>
    public static T ClampExcludeRange<T>(T value, T maxNeg, T minNeg, T zero, T minPos, T maxPos) where T : IComparable
    {
        if (value.CompareTo(zero) < 0)
        {
            if (value.CompareTo(maxNeg) < 0)
                return maxNeg;
            if (value.CompareTo(minNeg) > 0)
                return minNeg;
        }
        else
        {
            if (value.CompareTo(minPos) < 0)
                return maxNeg;
            if (value.CompareTo(maxPos) > 0)
                return minNeg;
        }
        return value;
    }


    /// <summary>
    /// Convert a string to have the first letter as uppercase.
    /// </summary>
    /// <param name="s"> The string to parse. </param>
    /// <returns> The modified string. </returns>
    public static string UppercaseFirst(string s)
    {
        char[] a = s.ToLower().ToCharArray();
        for (int i = 0; i < a.Length; i++)
        {
            a[i] = i == 0 || a[i - 1] == ' ' ? char.ToUpper(a[i]) : a[i];

        }
        return new string(a);
    }
    /// <summary>
    /// Returns the description attribute of an Enum.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string Description(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attribute.Length.Equals(default(int)) ? value.ToString() : ((DescriptionAttribute)attribute.First()).Description;
    }

    /// <summary>
    /// Get a fast approximate (kinda like Mathf.Approximate()). Used to determine if a float is 0 (or basicly close to 0 due to rounding).
    /// </summary>
    /// <param name="a"> First value. </param>
    /// <param name="b"> Second value. </param>
    /// <param name="threshold"> Threshold to check with. </param>
    /// <returns> Wheter or not the value was aprox the other value, within given threshold. </returns>
    public static bool FastApproximately(float a, float b, float threshold)
    {
        return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
    }

    /// <summary>
    /// Returns an int between min (inclusive) and max (exclusive) but excluding 1 number
    /// </summary>
    public static int RandomExcluding(int min, int max, int exlcuding)
    {
        IEnumerable<int> range = Enumerable.Range(min, max).Where(i => i != exlcuding);
        int index = UnityEngine.Random.Range(min, max - 1);
        return range.ElementAt(index);
    }
}