﻿using DeltaBotFour.Models;

namespace DeltaBotFour.ServiceInterfaces
{
    public interface ICommentReplyDetector
    {
        DB4ReplyResult DidDB4Reply(DB4Thing comment);
    }
}
