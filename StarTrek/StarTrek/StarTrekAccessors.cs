using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarTrek
{
    public class StarTrekAccessors
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StarTrekAccessors"/> class.
        /// Contains the <see cref="ConversationState"/> and associated <see cref="IStatePropertyAccessor{T}"/>.
        /// </summary>

        /// <param name="conversationState">The state object that stores the counter.</param>

        public StarTrekAccessors(ConversationState conversationState)
        {

            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
 
        }

        /// <summary>
        /// Gets or sets the <see cref="IStatePropertyAccessor{T}"/> for CounterState.
        /// </summary>
        public static string GameStartName { get; } = $"{nameof(StarTrekAccessors)}.GameState";
       
        /// <value>
        /// The accessor stores the turn count for the conversation.
        /// </value>

        public IStatePropertyAccessor<GameState> GameState { get; set; }


        // Conversation state is of type DialogState. Under the covers this is a serialized dialog stack.

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }

        public ConversationState ConversationState { get; }

    }

}
