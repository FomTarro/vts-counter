using System;
using System.Collections;
using System.Collections.Generic;

public static class IEnumeratorUtils
{

	private static Dictionary<Type, Array> _enumCache = new Dictionary<Type, Array>();

	/// <summary>
	/// Caching method for storing static lists of compiled enum values
	/// </summary>
	/// <typeparam name="T">The enum type to get the list of valeus for</typeparam>
	/// <returns></returns>
	public static T[] GetEnumValues<T>() where T : Enum
	{
		if (!_enumCache.ContainsKey(typeof(T)))
		{
			_enumCache.Add(typeof(T), Enum.GetValues(typeof(T)));
		}
		return _enumCache[typeof(T)] as T[];
	}

	/// <summary>
	/// Selects a random enum from a specified range, exclusive on both ends
	/// </summary>
	/// <param name="rangeStart"></param>
	/// <param name="rangeEnd"></param>
	/// <typeparam name="T">Type of enum to select from</typeparam>
	/// <returns></returns>
	public static T SelectRandomValue<T>(T rangeStart, T rangeEnd) where T : Enum
	{
		T start = rangeStart.CompareTo(rangeEnd) < 0 ? rangeStart : rangeEnd;
		T end = rangeStart.CompareTo(rangeEnd) < 0 ? rangeEnd : rangeStart;

		IEnumerator values = IEnumeratorUtils.GetEnumValues<T>().GetEnumerator();
		List<T> possibleValues = new List<T>();
		while (values.MoveNext())
		{
			T current = (T)values.Current;
			if (current.CompareTo(rangeStart) > 0 && current.CompareTo(rangeEnd) < 0)
			{
				possibleValues.Add(current);
			}
		}

		return possibleValues[new Random().Next(possibleValues.Count)];
	}

	/// <summary>
	/// Combine any number of IEnumerators into a single IEnumerator.
	/// 
	/// Useful for only requiring one yield for many discrete routines.
	/// </summary>
	/// <param name="enumerators">Any number of IEnumerators</param>
	/// <returns></returns>
	public static IEnumerator CombineEnumerators(params IEnumerator[] enumerators)
	{
		object[] nextObjects = new object[enumerators.Length];
		bool hasNext = false;
		bool shouldLoop = false;
		do
		{
			shouldLoop = false;
			for (int i = 0; i < nextObjects.Length; i++)
			{
				hasNext = enumerators[i].MoveNext();
				nextObjects[i] = hasNext ? enumerators[i].Current : null;
				shouldLoop |= hasNext;
			}
			yield return nextObjects;
		}
		while (shouldLoop);
	}

	public static IEnumerator GenericWaitForSeconds(float seconds)
	{
		float t = 0f;
		do
		{
			t += UnityEngine.Time.deltaTime;
			yield return null;
		}
		while (t < seconds);
	}
}
