﻿using RedditSharp.Things;

namespace DeltaBotFour.ServiceInterfaces
{
    public interface IDeltaAwarder
    {
        void Award(Comment comment);
    }
}
