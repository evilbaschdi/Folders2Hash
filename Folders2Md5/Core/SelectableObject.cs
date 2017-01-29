namespace Folders2Md5.Core
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectableObject<T>
    {
        /// <summary>
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// </summary>
        public T ObjectData { get; set; }

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
    }
}