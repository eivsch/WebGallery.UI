﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Pictures
{
    public class PictureResponse
    {
        public string Id { get; set; }
        public int GlobalSortOrder { get; set; }
        public int FolderSortOrder { get; set; }
    }
}
