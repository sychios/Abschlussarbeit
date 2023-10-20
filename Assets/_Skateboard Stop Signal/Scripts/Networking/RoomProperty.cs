public class RoomProperty
{
    public const string Condition = "C"; // {A,B}
    public const string ParticipantId = "ID"; // int
    public const string CurrentCanvas = "CC"; // {i, c, b}: i -> introduction, c -> creation, b -> break
    public const string CanvasInstructionCounter = "CV"; // float
    public const string LikertValue = "L"; // float, set the index (left to right, 0 to 6) of the toggle that is clicked
    public const string QuestionnaireIsOn = "Q"; // boolean
    public const string Questionnaire = "QI"; // string, "b" for break questionnaire, "f" for finish questionnaire
    public const string QuestionCounter = "QC"; // float
    public const string Language = "G"; // {G, E}
    
    public const string Arrow = "A"; // {"lt", "lf", "rt", "rf"} signals to show arrow for a certain time, is allways combined with char 't' (true) or 'f' (false) to signal if signal is on
    public const string Result = "R"; // result ("t", "f") with optional message id { "t0", "f0", ..}
    public const string SSD = "D"; // int used as index for the signal delay ladder
    public const string FeedbackGiven = "F"; // determines whether feedback was verbally given in the break

    public const string ScreenShot = "S"; // take screenshot
    public const string NewTaskArray = "N"; // Observer refreshes task array and empties the list in hiew view

    public const string GoTaskPerformance = "GP"; // Ratio of correct and bad go tasks
    public const string StopTaskPerformance = "SP"; // Ration of correct and bad stop tasks
    public const string ReactionTimeAverage = "RT"; // average reaction time
}
