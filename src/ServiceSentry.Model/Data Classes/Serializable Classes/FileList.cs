// -----------------------------------------------------------------------
//  <copyright file="FileList.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Model
{
    [DataContract, KnownType(typeof (ImplementedFileList))]
    public abstract class FileList : EquatableBase
    {
        [DataMember(Name = "ExternalFiles", IsRequired = true)]
        public abstract ObservableCollection<ExternalFile> Items { get; set; }

        public abstract void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e);
        public abstract void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e);

        #region Special Methods

        public static FileList Default
        {
            get { return new ImplementedFileList(); }
        }

        /// <summary>
        ///     Determines whether the specifies <see cref="object" /> is equal to the current <see cref="FileList" />.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="object" /> to compare with the current <see cref="FileList" />.
        /// </param>
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj)) return true;

            var p = (FileList) obj;
            var same = Items.SequenceEqual(p.Items);
            return same;
        }

        /// <summary>
        ///     Returns the hashcode for this <see cref="FileList" />.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap.
            {
                // pick two prime numbers
                const int seed = 19;
                var hash = 31;

                // be sure to check for nullity, etc.
                hash *= seed + (Items != null ? Items.GetHashCode() : 0);
                return hash;
            }
        }

        #endregion

        [DataContract]
        private sealed class ImplementedFileList : FileList
        {
            private ObservableCollection<ExternalFile> _items;

            public ImplementedFileList()
            {
                Items = new ObservableCollection<ExternalFile>();
                Items.CollectionChanged += Items_CollectionChanged;
            }

            public override ObservableCollection<ExternalFile> Items
            {
                get { return _items; }
                set
                {
                    if (_items == value) return;
                    _items = value;
                    OnPropertyChanged();
                }
            }

            #region Events

            public override void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (ExternalFile item in e.NewItems)
                    {
                        //_modifiedItems.Add(item);
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }

                if (e.OldItems == null) return;
                foreach (ExternalFile item in e.OldItems)
                {
                    //_modifiedItems.Add(item);
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }

            public override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e.PropertyName);
            }

            #endregion
        }
    }
}