// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using StarTrek.Dialogs;

namespace StarTrek
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service. 
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class StarTrekBot : IBot
    {
        private readonly StarTrekAccessors _accessors;
        private readonly DialogSet _dialogs;

        public StarTrekBot(ConversationState conversationState, StarTrekAccessors accessors, ILoggerFactory loggerFactory)
        {

            if (conversationState == null)
            {

                throw new System.ArgumentNullException(nameof(conversationState));

            }

            if (loggerFactory == null)
            {

                throw new System.ArgumentNullException(nameof(loggerFactory));

            }

            if (accessors == null)
            {

                throw new System.ArgumentNullException(nameof(accessors));

            }

            _accessors = accessors;

            _dialogs = new DialogSet(accessors.ConversationDialogState);
            _dialogs.Add(new WaterfallDialog("commandSelector", new WaterfallStep[] { ChoiceCommandStepAsync, ShowCommandStepAsync }));
            _dialogs.Add(new ChoicePrompt("commandPrompt"));
            _dialogs.Add(new TextPrompt("text"));
     
            var movement_slots = new List<SlotDetails>
            {

                new SlotDetails("quadrantCoordinates", "quadrantCoordinates"),
                new SlotDetails("sectorCoordinates", "sectorCoordinates"),

            };

            var quadrant_movement_slots = new List<SlotDetails>
            {
                new SlotDetails("QuadrantX", "text", "Please enter  Quadrant - X."),
                new SlotDetails("QuadrantY", "text", "Please enter  Quadrant - Y."),
            };

            var sector_movement_slots = new List<SlotDetails>
            {
                new SlotDetails("SectorX", "text", "Please enter  Sector - X."),
                new SlotDetails("SectorY", "text", "Please enter  Sector - Y."),
            };

            var photon_spread_slots = new List<SlotDetails>
            {
                new SlotDetails("Spread", "text", "How many missiles to fire."),
            };

            var photon_angle_slots = new List<SlotDetails>
            {
                new SlotDetails("Angle", "text", "0.00 - 7.00."),
            };

            var shield_energy_slots = new List<SlotDetails>
            {
                new SlotDetails("Energy", "text", "1 - 2000."),
            };

            _dialogs.Add(new SlotFillingDialog("quadrantCoordinates", quadrant_movement_slots));
            _dialogs.Add(new SlotFillingDialog("sectorCoordinates", sector_movement_slots));

            _dialogs.Add(new SlotFillingDialog("movement", movement_slots));
            _dialogs.Add(new WaterfallDialog("coordinates", new WaterfallStep[] { CoordinateDialogAsync, ProcessCoordinateResultsAsync }));

            _dialogs.Add(new SlotFillingDialog("shields", shield_energy_slots));

        }

        /// <summary>
        /// Prompts the user for input by sending a <see cref="ChoicePrompt"/> so the user may select their
        /// choice from a list of options.

        /// </summary>
        /// <param name="step">A <see cref="WaterfallStepContext"/> provides context for the current waterfall step.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="DialogTurnResult"/> to communicate some flow control back to the containing WaterfallDialog.</returns>

        private static async Task<DialogTurnResult> ChoiceCommandStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {

            return await step.PromptAsync("commandPrompt", GenerateOptions(step.Context.Activity), cancellationToken);

        }

        /// <summary>
        /// Creates options for a <see cref="ChoicePrompt"/> so user select the command.
        /// </summary>
        /// <param name="activity">The message activity the bot received.</param>
        /// <returns>A <see cref="PromptOptions"/> to be used in a prompt.</returns>

        /// <remarks>Related type <see cref="Choice"/>.</remarks>

        private static PromptOptions GenerateOptions(Activity activity)
        {
            // Create options for the prompt

            var options = new PromptOptions()
            {

                Prompt = activity.CreateReply("ENTER COMMAND:"),

                Choices = new List<Choice>(),

            };

            // Add the choices for the prompt.

            options.Choices.Add(new Choice() { Value = "SET COURSE" });
            options.Choices.Add(new Choice() { Value = "SHORT RANGE SENSOR SCAN" });
            options.Choices.Add(new Choice() { Value = "LONG RANGE SENSOR SCAN" });
            options.Choices.Add(new Choice() { Value = "FIRE PHASERS" });
            options.Choices.Add(new Choice() { Value = "FIRE PHOTON TORPEDOES" });
            options.Choices.Add(new Choice() { Value = "SHIELD CONTROL" });
            options.Choices.Add(new Choice() { Value = "DAMAGE CONTROL REPORT" });
            options.Choices.Add(new Choice() { Value = "CALL ON LIBRARY COMPUTER" });
            options.Choices.Add(new Choice() { Value = "BLOW-UP SHIP" });

            return options;

        }

        /// <summary>
        /// Every conversation turn calls this method.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = await _accessors.GameState.GetAsync(turnContext, () => new GameState());

            if (state.StarMap == "")
            {
                state.StarMap = ObjectToString(new StarMap());
            }

            state.TurnCount = state.TurnCount + 1;

            await _accessors.GameState.SetAsync(turnContext, state);
            await _accessors.ConversationState.SaveChangesAsync(turnContext);

            Console.WriteLine($"Klingons: {state.TurnCount}");

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await this._dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                if (results.Status == DialogTurnStatus.Empty)
                {

                    await dialogContext.BeginDialogAsync("commandSelector", cancellationToken: cancellationToken);

                }
                else
                {
                    Console.WriteLine($"Command [Loop]: {turnContext.Activity.Text}");
                }

            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {

                    await SendWelcomeMessageAsync(turnContext, cancellationToken);

                }

            }
            else
            {

                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected", cancellationToken: cancellationToken);

            }

            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

        }

        /// <summary>
        /// Sends a welcome message to the user.
        /// </summary>

        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {

            foreach (var member in turnContext.Activity.MembersAdded)
            {

                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = turnContext.Activity.CreateReply();

                    reply.Text = $"{member.Name} - ENTER 'COMMAND' to START:";

                    await turnContext.SendActivityAsync(reply, cancellationToken);

                }

            }

        }

        /// <summary>
        /// This method processes the command
        /// </summary>
        /// <param name="step">A <see cref="WaterfallStepContext"/> provides context for the current waterfall step.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="DialogTurnResult"/> indicating the turn has ended.</returns>
        /// <remarks>Related types <see cref="Attachment"/> and <see cref="AttachmentLayoutTypes"/>.</remarks>
        private async Task<DialogTurnResult> ShowCommandStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            var state = await _accessors.GameState.GetAsync(step.Context, () => new GameState());

            Console.WriteLine($"[Dialog] Klingons: {state.Klingons} : {state.TurnCount}");

            var text = step.Context.Activity.Text.ToLowerInvariant().Split(' ')[0];
            Console.WriteLine($"[Choice] Command: {text} ");

            // Reply to the activity we received with an activity.
            var reply = step.Context.Activity.CreateReply();

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments on the activity.

            reply.Attachments = new List<Attachment>();

            if (text.StartsWith("2") || text.StartsWith("SR"))
            {
                // Display a Short Range Card

                reply.Attachments.Add(GetShortRangeCard(state).ToAttachment());

                await step.Context.SendActivityAsync(reply, cancellationToken);

            }

            if (text.StartsWith("3") || text.StartsWith("LR"))
            {
                // Display a Short Range Card

                reply.Attachments.Add(GetLongRangeCard(state).ToAttachment());

                await step.Context.SendActivityAsync(reply, cancellationToken);

            }

            // Send the card(s) to the user as an attachment to the activity


            // Give the user instructions about what to do next

            if (text.StartsWith("9"))
            {
                await step.Context.SendActivityAsync("SHIP EXPLODED - GAME OVER", cancellationToken: cancellationToken);

                return await step.EndDialogAsync(cancellationToken: cancellationToken);

            }

            if (text.StartsWith("1"))
            {
                return await step.BeginDialogAsync("coordinates", null, cancellationToken);
            }

            return await step.BeginDialogAsync("commandSelector", cancellationToken: cancellationToken);

        }
        /// </summary>
        /// <param name="step">A <see cref="WaterfallStepContext"/> provides context for the current waterfall step.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="DialogTurnResult"/> to communicate some flow control back to the containing WaterfallDialog.</returns>

        private static async Task<DialogTurnResult> CoordinateDialogAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            return await step.BeginDialogAsync("movement", null, cancellationToken);

        }
        /// <summary>
        /// This method move the ship to a new location
        /// </summary>
        /// <param name="step">A <see cref="WaterfallStepContext"/> provides context for the current waterfall step.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="DialogTurnResult"/> indicating the command has ended.</returns>
        /// <remarks>Related types <see cref="Attachment"/> and <see cref="AttachmentLayoutTypes"/>.</remarks>
        private async Task<DialogTurnResult> ProcessCoordinateResultsAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {

            if (step.Result is IDictionary<string, object> result && result.Count > 0)
            {
                var state = await _accessors.GameState.GetAsync(step.Context, () => new GameState());
                StarMap StarMap = (StarMap)StringToObject(state.StarMap);

                var quadrant = (IDictionary<string, object>)result["quadrantCoordinates"];
                var sector = (IDictionary<string, object>)result["sectorCoordinates"];

                StarMap.MoveEnterprise(Convert.ToInt32(quadrant["QuadrantX"]),
                                       Convert.ToInt32(quadrant["QuadrantY"]),
                                       Convert.ToInt32(sector["SectorX"]),
                                       Convert.ToInt32(sector["SectorY"]));

                state.StarMap = ObjectToString(StarMap);

                await _accessors.GameState.SetAsync(step.Context, state);
                await _accessors.ConversationState.SaveChangesAsync(step.Context);

                await step.Context.SendActivityAsync(MessageFactory.Text($"Moving To: {quadrant["QuadrantX"]}:{quadrant["QuadrantY"]}, {sector["SectorX"]}:{sector["SectorY"]} "));
            }

            return await step.BeginDialogAsync("commandSelector", cancellationToken: cancellationToken);

        }

        /// <summary>
        /// Creates a <see cref="HeroCard"/>.
        /// </summary>
        /// <returns>A <see cref="HeroCard"/> for Short Rabge Sensors</returns>
        /// <remarks>Related types <see cref="CardImage"/>, <see cref="CardAction"/>,
        /// and <see cref="ActionTypes"/>.</remarks>
        private HeroCard GetShortRangeCard(GameState gameState)
        {
            string url = "";

            StarMap StarMap = (StarMap)StringToObject(gameState.StarMap);

            var Enterprise = StarMap.Enterprise;

            Console.WriteLine($"Enterprise: {Enterprise.QX} : {Enterprise.QY} : {Enterprise.SX} : {Enterprise.SY}");

            var Quadrants = StarMap.Quadrants;
            var Quadrant = StarMap.Quadrants[Enterprise.QX, Enterprise.QY];

            using (MagickImage image = new MagickImage(new MagickColor("#000000"), 512, 256))
            {
                image.Format = MagickFormat.Png;

                Drawables drawables = new Drawables()
                  // Draw text on the image
                  .FontPointSize(24)
                  .Font("Courier New")
                  .Rectangle(5, 5, 248, 248)
                  .StrokeColor(MagickColors.Green)
                  .FillColor(MagickColors.Black)
                  .TextAlignment(TextAlignment.Left);

                bool klingonPresent = false;

                for (var sX = 0; sX < 8; sX++)
                {
                    for (var sY = 0; sY < 8; sY++)
                    {

                        Piece piece = Quadrant.Sector[sX, sY];

                        if (piece.Type == Piece.Pieces.star)
                        {
                            drawables = drawables.Text(sX * 28 + 30, sY * 28 + 30, "*");

                            Console.WriteLine($"Star {Enterprise.QX} : {Enterprise.QY} : {sX} : {sY}");

                        }
                        else if (piece.Type == Piece.Pieces.enterprise)
                        {
                            drawables = drawables.Text(sX * 28 + 30, sY * 28 + 30, "E");

                            Console.WriteLine($"Enterprise {Enterprise.QX} : {Enterprise.QY} : {sX} : {sY}");
                        }
                        else if (piece.Type == Piece.Pieces.klingon)
                        {
                            klingonPresent = true;

                            drawables = drawables.Text(sX * 28 + 30, sY * 28 + 30, "K");

                            Console.WriteLine($"Klingon {Enterprise.QX} : {Enterprise.QY} : {sX} : {sY}");

                        }
                        else if (piece.Type == Piece.Pieces.stardock)
                        {
                            drawables = drawables.Text(sX * 28 + 30, sY * 28 + 30, "D");

                            Console.WriteLine($"StarDoc {Enterprise.QX} : {Enterprise.QY} : {sX} : {sY}");

                        }

                    }

                }

                drawables = drawables.Text(270, 24, $"Condition: {(klingonPresent ? "Red" : "Green")}");
                drawables = drawables.FontPointSize(12)
                                     .Rectangle(410, 190, 500, 245);

                for (var iX = -1; iX < 2; iX++)
                {
                    for (var iY = -1; iY < 2; iY++)
                    {
                        var qX = Enterprise.QX + iX;
                        var qY = Enterprise.QY + iY;

                        if (qX <= 0 || qX > 7 || qY <= 0 || qY > 7)
                        {
                            drawables = drawables.Text((iX + 1) * 28 + 417, (iY + 1) * 16 + 205, "000");
                        }
                        else
                        {
                            drawables = drawables.Text((iX + 1) * 28 + 417, (iY + 1) * 16 + 205,
                                                        StarMap.GetQuadrantSummary(qX, qY));
                        }

                    }

                }

                drawables.Draw(image);

                url = @"data:image/png;base64," + image.ToBase64();

            }

            var heroCard = new HeroCard
            {
                Title = $"Quadrant: {Enterprise.QX},{Enterprise.QY} Sector: {Enterprise.SX},{Enterprise.SY}",
                Images = new List<CardImage> { new CardImage(url) },
                Buttons = new List<CardAction> { },
            };

            return heroCard;

        }

        /// <summary>
        /// Creates a <see cref="HeroCard"/>.
        /// </summary>
        /// <returns>A <see cref="HeroCard"/> for Long Range Sensors</returns>
        /// <remarks>Related types <see cref="CardImage"/>, <see cref="CardAction"/>,
        /// and <see cref="ActionTypes"/>.</remarks>
        private HeroCard GetLongRangeCard(GameState gameState)
        {
            string url = "";

            StarMap StarMap = (StarMap)StringToObject(gameState.StarMap);

            var Enterprise = StarMap.Enterprise;

            Console.WriteLine($"Enterprise: {Enterprise.QX} : {Enterprise.QY} : {Enterprise.SX} : {Enterprise.SY}");

            var Quadrants = StarMap.Quadrants;
            var Quadrant = StarMap.Quadrants[Enterprise.QX, Enterprise.QY];

            using (MagickImage image = new MagickImage(new MagickColor("#000000"), 512, 256))
            {
                image.Format = MagickFormat.Png;

                Drawables drawables = new Drawables()
                  // Draw text on the image
                  .FontPointSize(18)
                  .Font("Courier New")
                  .Rectangle(5, 5, 390, 210)
                  .StrokeColor(MagickColors.Green)
                  .FillColor(MagickColors.Black)
                  .TextAlignment(TextAlignment.Left);

                for (var qX = 0; qX < 8; qX++)
                {
                    for (var qY = 0; qY < 8; qY++)
                    {
                        var summary = StarMap.GetQuadrantSummary(qX, qY);

                        if (Enterprise.QX == qX && Enterprise.QY == qY)
                        {
                            summary += "*";
                        }

                        drawables = drawables.Text(qX * 48 + 12, qY * 24 + 28, summary);

                    }

                }

                drawables = drawables.Text(10, 240, "* - Current Position");
                drawables = drawables.Text(396, 18, "Condition:");

                if (StarMap.CheckQuadrant(Piece.Pieces.klingon, Enterprise.QX, Enterprise.QY) > 0)
                { 
                    drawables = drawables.Text(396, 42, "Red");
                }
                else
                {
                    drawables = drawables.Text(396, 42, "Green");
                }

                drawables.Draw(image);

                url = @"data:image/png;base64," + image.ToBase64();

            }

            var heroCard = new HeroCard
            {
                Title = $"Quadrant: {Enterprise.QX},{Enterprise.QY} Sector: {Enterprise.SX},{Enterprise.SY}",
                Images = new List<CardImage> { new CardImage(url) },
                Buttons = new List<CardAction> { },
            };

            return heroCard;

        }

        public string ObjectToString(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }

        }

        public object StringToObject(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                return new BinaryFormatter().Deserialize(ms);
            }
        }

    }

}