# language: sv

Egenskap: Inloggning
  Som besökare vill jag kunna logga in
  så att jag kommer åt mina quiz och min statistik.

  Scenario: Användaren loggar in med giltiga uppgifter
    Givet att jag är på startsidan
    När jag öppnar inloggningsrutan
    Och jag fyller i "user" som användarnamn
    Och jag fyller i "User123!" som lösenord
    Och jag skickar inloggningsformuläret
    Så ska jag vara inloggad som "user"

  Scenario: Fel lösenord ger ett felmeddelande
    Givet att jag är på startsidan
    När jag öppnar inloggningsrutan
    Och jag fyller i "user" som användarnamn
    Och jag fyller i "FelLosen1" som lösenord
    Och jag skickar inloggningsformuläret
    Så ska jag se ett felmeddelande
    Och jag ska fortfarande vara utloggad