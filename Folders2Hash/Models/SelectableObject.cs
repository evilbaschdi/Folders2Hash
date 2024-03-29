﻿namespace Folders2Hash.Models;

/// <summary>
/// </summary>
/// <typeparam name="T"></typeparam>
public class SelectableObject<T>
{
    /// <summary>
    /// </summary>
    /// <param name="objectData"></param>
    public SelectableObject(T objectData)
    {
        ObjectData = objectData;
    }

    /// <summary>
    /// </summary>
    /// <param name="objectData"></param>
    /// <param name="isSelected"></param>
    public SelectableObject(T objectData, bool isSelected)
    {
        IsSelected = isSelected;
        ObjectData = objectData;
    }

    /// <summary>
    /// </summary>
    public bool IsSelected { get; init; }

    /// <summary>
    /// </summary>
    public T ObjectData { get; }
}