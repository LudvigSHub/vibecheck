# language: sv

Egenskap: Skyddade sidor
  Sidor som kräver konto ska inte gå att nå genom att skriva in adressen.

  Scenario: Utloggad användare kan inte nå quizsidan
    Givet att jag inte är inloggad
    När jag går till "/quiz"
    Så ska inloggningsrutan visas med texten "Logga in för att nå den sidan."
    Och jag ska inte se quizsidan
    Och jag ska vara kvar på startsidan