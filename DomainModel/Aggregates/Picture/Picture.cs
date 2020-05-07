using DomainModel.Common;
using DomainModel.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel.Aggregates.Picture
{
    public class Picture : Entity, IAggregateRoot
    {
        int _globalSortOrder;
        int _folderSortOrder;

        public virtual int GlobalSortOrder => _globalSortOrder;
        public virtual int FolderSortOrder => _folderSortOrder;

        private Picture(string id)
        {
            Id = id;
        }

        public static Picture Create(string id, int globalSortOrder, int folderSortOrder)
        {
            return new Picture(id) { _globalSortOrder = globalSortOrder, _folderSortOrder = folderSortOrder };
        }
    }
}
