﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using RestaurantBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RestaurantBot
{
    [LuisModel("e2fb5dc3-8e97-4d8c-a068-54d38c6545e4", "f017f470eff645c898f139b24dc3948c")]
    [Serializable]
    public class MainDialog : LuisDialog<Object>
    {
        [LuisIntent("None")]
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm not sure what you want.");
            context.Wait(MessageReceived);
        }
        [LuisIntent("ViewMenu")]
        public async Task ViewMenu(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            try
            {
                var message = await activity;
                var reply = context.MakeMessage();
                var attachment = GetMenuAttachment();
                if (message.ChannelId == "facebook")
                {
                    reply.ChannelData = new FbChannelData()
                    {
                        Attachment = attachment
                    };
                    await context.PostAsync(reply);
                }
                else if (message.ChannelId == "telegram")
                {
                    // The Attachments property allows you to send and receive images and other content
                    reply.Attachments = new List<Attachment>()
                    {
                        new Attachment()
                        {
                            ContentUrl = "http://restaurantmenudesignstyle.com/wp-content/uploads/2016/01/template-restaurant-menu-word.menu_.jpg",
                            ContentType = "image/jpg",
                            Name = "Mainmenu.jpg"
                        }
                    };
                    await context.PostAsync("Here's our full menu: ");
                    await context.PostAsync(reply);
                }
                else
                {
                    await context.PostAsync("Show menu in other channels");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Sorry something went wrong.");
            }
            finally
            {
                context.Wait(MessageReceived);
            }
        }
        private FbAttachment GetMenuAttachment()
        {
            return new FbAttachment()
            {
                Payload = new FbAttachmentPayload()
                {
                    Elements = new FbCard[]
                    {
                        new FbCard()
                        {
                            Title = "Menu",
                            ImageUrl = "http://dhcdc.com/wordpress/wp-content/uploads/2013/06/menuicon.png",
                            Subtitle = "Which menu would you like?",
                            Buttons = new FbButton[]
                            {
                                new FbButton()
                                {
                                    Type = "web_url",
                                    Url = "http://nandosmexicancafe.com/menu-2/",
                                    Title = "All food items",
                                    WebViewRatio = "compact"
                                },
                                new FbButton()
                                {
                                    Type = "web_url",
                                    Url = "http://www.ilovepeppertree.com/menu/",
                                    Title = "Vegetarian and Halal",
                                    WebViewRatio = "tall"
                                }
                            }
                        }
                    }
                }
            };
        }
        [LuisIntent("MakeBooking")]
        public async Task MakeBooking(IDialogContext context, LuisResult result)
        {
            try
            {
                var entities = new List<EntityRecommendation>(result.Entities);
                //Chronic.Parser parser = new Chronic.Parser();
                //EntityRecommendation date = new EntityRecommendation();
                //result.TryFindEntity("builtin.datetime.date", out date);
                //var dateResult = parser.Parse(date.Entity);
                EntityRecommendation entityDate;
                EntityRecommendation entityTime;
                EntityRecommendation entityNumPpl;
                result.TryFindEntity("numppl", out entityNumPpl);
                result.TryFindEntity("builtin.datetime.date", out entityDate);
                result.TryFindEntity("builtin.datetime.time", out entityTime);

                if ((entityDate != null) & (entityTime != null))
                    entities.Add(new EntityRecommendation(type: "Date") { Entity = entityDate.Entity + " " + entityTime.Entity });

                else if (entityDate != null)
                    entities.Add(new EntityRecommendation(type: "Date") { Entity = entityDate.Entity });

                else if (entityTime != null)
                    entities.Add(new EntityRecommendation(type: "Time") { Entity = entityTime.Entity });

                await context.PostAsync("Sure thing - I'll need some details from you.");
                var bookingForm = new FormDialog<BookingForm>(new BookingForm(), BookingForm.BuildForm, FormOptions.PromptInStart, entities);
                context.Call(bookingForm, BookingFormComplete);
            }
            catch (Exception)
            {
                await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
                context.Wait(MessageReceived);
            }
        }
        private async Task BookingFormComplete(IDialogContext context, IAwaitable<BookingForm> result)
        {
            try
            {
                var bookingform = await result;
                SaveBooking(context, bookingform);
                await context.PostAsync("Your booking is confirmed.");
                //var booking = context.UserData.Get<Booking>("booking");
                //await context.PostAsync("You current booking is: " + booking.BookingDateTime);
                //Go back to main menu
            }
            catch (FormCanceledException)
            {
                await context.PostAsync("I did not make your booking.");
            }
            catch (Exception)
            {
                await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
            }
            finally
            {
                context.Wait(MessageReceived);
            }
        }
        private void SaveBooking(IDialogContext context, BookingForm bookingform)
        {
            var booking = new Booking();
            if (bookingform.Time != null)
            {
                // Time stated separately
                var time = bookingform.Time.GetValueOrDefault();
                booking.BookingDateTime = bookingform.Date.Date + time.TimeOfDay;
            }
            else
            {
                booking.BookingDateTime = bookingform.Date;
            }
            booking.Name = bookingform.Name;
            booking.NumPeople = bookingform.NumPeople;
            booking.PhNum = bookingform.PhNum;
            booking.Requests = bookingform.Requests;
            context.UserData.SetValue<Booking>("booking", booking);
        }
        [LuisIntent("CancelBooking")]
        public async Task CancelBooking(IDialogContext context, LuisResult result)
        {
            Booking booking;
            if (context.UserData.TryGetValue<Booking>("booking", out booking))
            {
                PromptDialog.Confirm(
                       context,
                       AfterCancelBooking,
                       "Are you sure you want to cancel your current booking for " + booking.BookingDateTime + "? (Y/N)",
                       "Cancel current booking? (Y/N)",
                       promptStyle: PromptStyle.Auto);
            }
            else
            {
                await context.PostAsync("You have no current bookings.");
                context.Wait(MessageReceived);
            }
        }
        public async Task AfterCancelBooking(IDialogContext context, IAwaitable<bool> argument)
        {
            try
            {
                var confirm = await argument;
                if (confirm)
                {
                    context.UserData.RemoveValue("booking");
                    await context.PostAsync("Your booking has been cancelled.");
                }
                else
                {
                    await context.PostAsync("I didn't cancel your booking.");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Something went wrong. Eek");
            }
            finally
            {
                context.Wait(MessageReceived);
            }
        }
        [LuisIntent("ViewBooking")]
        public async Task ViewBooking(IDialogContext context, LuisResult result)
        {
            try
            {
                Booking booking;
                if (context.UserData.TryGetValue<Booking>("booking", out booking))
                {
                    await context.PostAsync("You have a booking at " + booking.BookingDateTime + ".");
                }
                else
                {
                    await context.PostAsync("You have no current bookings.");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Something went wrong, sorry :(");
            }
            finally
            {
                context.Wait(MessageReceived);
            }
        }
    }
}