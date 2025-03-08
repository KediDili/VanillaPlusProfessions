#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;

namespace VanillaPlusProfessions.Compatibility;


/// <summary>
/// A page created event is emitted whenever a new page
/// is created for a tab by Better Game Menu.
/// </summary>
public interface IPageCreatedEvent
{

    /// <summary>
    /// The Better Game Menu instance involved in the event. You
    /// can use <see cref="IBetterGameMenuApi.AsMenu(IClickableMenu)"/>
    /// to get a more useful interface for this menu.
    /// </summary>
    IClickableMenu Menu { get; }

    /// <summary>
    /// The id of the tab the page was created for.
    /// </summary>
    string Tab { get; }

    /// <summary>
    /// The id of the provider the page was created with.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// The new page that was just created.
    /// </summary>
    IClickableMenu Page { get; }

    /// <summary>
    /// If the page was previously created and is being replaced,
    /// this will be the old page instance. Otherwise, this will
    /// be <c>null</c>.
    /// </summary>
    IClickableMenu? OldPage { get; }

}


/// <summary>
/// This interface represents a Better Game Menu. 
/// </summary>
public interface IBetterGameMenu
{
    /// <summary>
    /// The <see cref="IClickableMenu"/> instance for this game menu. This is
    /// the same object, but with a different type. This property is included
    /// for convenience due to how API proxying works.
    /// </summary>
    IClickableMenu Menu { get; }

    /// <summary>
    /// Whether or not the menu is currently drawing itself. This is typically
    /// always <c>false</c> except when viewing the <c>Map</c> tab.
    /// </summary>
    bool Invisible { get; set; }

    /// <summary>
    /// A list of ids of the currently visible tabs.
    /// </summary>
    IReadOnlyList<string> VisibleTabs { get; }

    /// <summary>
    /// The id of the currently active tab.
    /// </summary>
    string CurrentTab { get; }

    /// <summary>
    /// The <see cref="IClickableMenu"/> instance for the currently active tab.
    /// This may be <c>null</c> if the page instance for the currently active
    /// tab is still being initialized.
    /// </summary>
    IClickableMenu? CurrentPage { get; }

    /// <summary>
    /// Whether or not the currently displayed page is an error page. Error
    /// pages are used when a tab implementation's GetPageInstance method
    /// throws an exception.
    /// </summary>
    bool CurrentTabHasErrored { get; }

    /// <summary>
    /// Try to get the source for the specific tab.
    /// </summary>
    /// <param name="target">The id of the tab to get the source of.</param>
    /// <param name="source">The unique ID of the mod that registered the
    /// implementation being used, or <c>stardew</c> if the base game's
    /// implementation is being used.</param>
    /// <returns>Whether or not the tab is registered with the system.</returns>
    bool TryGetSource(string target, [NotNullWhen(true)] out string? source);

    /// <summary>
    /// Try to get the <see cref="IClickableMenu"/> instance for a specific tab.
    /// </summary>
    /// <param name="target">The id of the tab to get the page for.</param>
    /// <param name="page">The page instance, if one exists.</param>
    /// <param name="forceCreation">If set to true, an instance will attempt to
    /// be created if one has not already been created.</param>
    /// <returns>Whether or not a page instance for that tab exists.</returns>
    bool TryGetPage(string target, [NotNullWhen(true)] out IClickableMenu? page, bool forceCreation = false);

    /// <summary>
    /// Attempt to change the currently active tab to the target tab.
    /// </summary>
    /// <param name="target">The id of the tab to change to.</param>
    /// <param name="playSound">Whether or not to play a sound.</param>
    /// <returns>Whether or not the tab was changed successfully.</returns>
    bool TryChangeTab(string target, bool playSound = true);

    /// <summary>
    /// Force the menu to recalculate the visible tabs. This will not recreate
    /// <see cref="IClickableMenu"/> instances, but can be used to cause an
    /// inactive tab to be removed, or a previously hidden tab to be added.
    /// This can also be used to update tab decorations if necessary.
    /// </summary>
    /// <param name="target">Optionally, a specific tab to update rather than
    /// updating all tabs.</param>
    void UpdateTabs(string? target = null);

}


/// <summary>
/// This enum is included for reference and has the order value for
/// all the default tabs from the base game. These values are intentionally
/// spaced out to allow for modded tabs to be inserted at specific points.
/// </summary>
public enum VanillaTabOrders
{
    Inventory = 0,
    Skills = 20,
    Social = 40,
    Map = 60,
    Crafting = 80,
    Animals = 100,
    Powers = 120,
    Collections = 140,
    Options = 160,
    Exit = 200
}


public interface IBetterGameMenuApi
{

    #region Menu Class Access

    /// <summary>
    /// Attempt to cast the provided menu into an <see cref="IBetterGameMenu"/>.
    /// This can be useful if you're working with a menu that isn't currently
    /// assigned to <see cref="Game1.activeClickableMenu"/>.
    /// </summary>
    /// <param name="menu">The menu to attempt to cast</param>
    IBetterGameMenu? AsMenu(IClickableMenu menu);

    /// <summary>
    /// Get the current page of the provided Better Game Menu instance. If the
    /// provided menu is not a Better Game Menu, or a page is not ready, then
    /// return <c>null</c> instead.
    /// </summary>
    /// <param name="menu">The menu to get the page from.</param>
    IClickableMenu? GetCurrentPage(IClickableMenu menu);

    /// <summary>
    /// Create a new Better Game Menu instance and return it.
    /// </summary>
    /// <param name="defaultTab">The tab that the menu should be opened to.</param>
    /// <param name="playSound">Whether or not a sound should play.</param>
    IClickableMenu CreateMenu(
        string? defaultTab = null,
        bool playSound = false
    );

    #endregion

    #region Menu Events

    public delegate void PageCreatedDelegate(IPageCreatedEvent evt);

    /// <summary>
    /// This event fires whenever a new page instance is created. This can happen
    /// the first time a page is accessed, whenever something calls
    /// <see cref="TryGetPage(string, out IClickableMenu?, bool)"/> with the
    /// <c>forceCreation</c> flag set to true, or when the menu has been resized
    /// and the tab implementation's <c>OnResize</c> method returned a new
    /// page instance.
    /// </summary>
    void OnPageCreated(PageCreatedDelegate handler, EventPriority priority = EventPriority.Normal);

    /// <summary>
    /// Unregister a handler for the PageCreated event.
    /// </summary>
    void OffPageCreated(PageCreatedDelegate handler);

    #endregion

}
