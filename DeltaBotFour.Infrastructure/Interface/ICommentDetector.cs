﻿using DeltaBotFour.Models;

namespace DeltaBotFour.Infrastructure.Interface
{
    public interface ICommentDetector
    {
        DB4ReplyResult DidDB4MakeStickyComment(DB4Thing post);
        DB4ReplyResult DidDB4Reply(DB4Thing comment);
    }
}
