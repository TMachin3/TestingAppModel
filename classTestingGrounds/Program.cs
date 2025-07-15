using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classTestingGrounds
{
    enum questionType { textBox, checkBox, radioButton }
    struct QuizResults
    {
        public Grade FinalGrade { get; set; }
        public int TotalScore { get; set; }
        public int CorrectAnswersCount { get; set; }
        public int IncorrectAnswersCount { get; set; }
        public int TotalPenalty { get; set; }
    }
    class Answer
    {
        public string Content { get; set; } = string.Empty;
        public int Score { get; set; }
        public bool IsTrue { get; set; }
        public bool IsUserSelected { get; set; } = false;
        public virtual int Award
        {
            get
            {
                return Score > 0 ? Score : 0;
            }
        }
        public virtual int Penalty
        {
            get
            {
                return Score < 0 ? Math.Abs(Score) : 0;
            }
        }
    }
    class Question
    {
        public string Content { get; set; } = string.Empty;
        public Answer[] Answers { get; set; }
        public questionType QuestionType
        {
            get; set;
            /* Old virtual method, replaced with static declaration for flexibility, kept for reference only
            get
            {
                if (this.Answers.Length == 0) { return questionType.textBox; }
                else
                {
                    int trueAnswerCount = 0;
                    foreach (Answer answer in Answers) { if (answer.IsTrue) { trueAnswerCount++; } }
            If there are only true answers, define as textBox
                    if (this.Answers.Length == trueAnswerCount) { return questionType.textBox; }
            If there is only one true answer, define as radioButton
                    else if (trueAnswerCount == 1) { return questionType.radioButton; }
            If there are multiple correct answers with any incorrect ones, define as checkBox
                    else { return questionType.checkBox; }
                }
            */
        }

        //Fetching Question Score, Award or Penalty will calculate total score applied based on IsUserSelected flag
        public virtual int Score
        {
            get
            {
                if (Answers == null) return 0;
                else
                {
                    int totalScore = 0;
                    switch (this.QuestionType)
                    {
                        case questionType.textBox:
                            {
                                var selectedTbAnswer = Answers.FirstOrDefault(a => a.IsUserSelected);
                                //Return score value, including negative points, if such answer exists (specifically penalized options)
                                if (selectedTbAnswer != null) { totalScore += selectedTbAnswer.Score; break; }
                            }
                            break;
                        case questionType.radioButton:
                            {
                                var selectedRbAnswer = Answers.FirstOrDefault(a => a.IsUserSelected);
                                if (selectedRbAnswer != null && selectedRbAnswer.IsTrue) { totalScore += selectedRbAnswer.Score; break; }
                            }
                            break;
                        case questionType.checkBox:
                            {
                                bool voidAward = false; //Flag for voiding award score if incorrect answers are selected
                                foreach (var answer in Answers)
                                {
                                    if (answer.IsUserSelected && !answer.IsTrue) voidAward = true; break;
                                }
                                if (voidAward)
                                {
                                    foreach (var answer in Answers)
                                    {
                                        //Count only penalty if award is voided
                                        if (!answer.IsTrue && answer.IsUserSelected) totalScore += answer.Score;
                                    }
                                }
                                else
                                {
                                    foreach (var answer in Answers)
                                    {
                                        if (answer.IsUserSelected) totalScore += answer.Score;
                                    }
                                }
                            }
                            break;
                    }
                    return totalScore;
                }
            }
        }
        public virtual int Award
        {
            get
            {
                int totalAward = this.Score;
                if (totalAward > 0) return totalAward;
                else return 0;
            }
        }
        public virtual int Penalty
        {
            get
            {
                int totalPenalty = this.Score;
                if (totalPenalty < 0) return Math.Abs(totalPenalty);
                else return 0;
            }
        }
        public virtual bool IsCorrect
        {
            get
            {
                if (Answers == null) return false;
                else
                {
                    switch (this.QuestionType)
                    {
                        case questionType.textBox:
                            {
                                var selectedTbAnswer = Answers.FirstOrDefault(a => a.IsUserSelected);
                                if (selectedTbAnswer != null && selectedTbAnswer.IsTrue) return true;
                                else return false;
                            }
                        case questionType.radioButton:
                            {
                                var selectedRbAnswer = Answers.FirstOrDefault(a => a.IsUserSelected);
                                if (selectedRbAnswer != null && selectedRbAnswer.IsTrue) return true;
                                else return false;
                            }
                        case questionType.checkBox:
                            {
                                bool voidAward = false; //Flag for voiding award score if incorrect answers are selected
                                foreach (var answer in Answers)
                                {
                                    if (answer.IsUserSelected && !answer.IsTrue) voidAward = true; break;
                                }
                                return !voidAward;
                            }
                        default: return false;
                    }
                }
            }
        }
    }
    class Grade
    {
        public string Title { get; set; }
        public int MinScore { get; set; }
        public int MinCorrectAnswers { get; set; }
        public int FailPenalty { get; set; } //Penalty points to fail the grade
        public int FailIncorrectAnswers { get; set; } //Number of wrong answers to fail the grade
        public bool IsPassingGrade { get; set; }
    }
    class Questionnaire
    {
        public Question[] Questions { get; set; }
        public Grade[] Grades { get; set; }
        public int DefaultAward { get; set; } = 1;
        public int DefaultPenalty { get; set; } = 0;
        public void SetDefaultPercentageGrades()
        {
            int maxPossibleScore = 0;
            int maxPossibleCorrectAnswers = 0;

            if (Questions != null)
            {
                foreach (var question in Questions)
                {
                    if (question.Answers != null)
                    {
                        // For each question, sum up the scores of its true answers for max possible score.
                        // For text box questions, assume the 'IsTrue' answer is the only one counted towards score.
                        if (question.QuestionType == questionType.textBox)
                        {
                            var trueAnswer = question.Answers.FirstOrDefault(a => a.IsTrue);
                            if (trueAnswer != null)
                            {
                                maxPossibleScore += trueAnswer.Score;
                                maxPossibleCorrectAnswers++;
                            }
                        }
                        else // For radioButton and checkBox
                        {
                            maxPossibleScore += question.Answers.Where(a => a.IsTrue).Sum(a => a.Score);
                            maxPossibleCorrectAnswers += question.Answers.Count(a => a.IsTrue);
                        }
                    }
                }
            }

            int score90Percent = (int)Math.Ceiling(maxPossibleScore * 0.90);
            int score75Percent = (int)Math.Ceiling(maxPossibleScore * 0.75);
            int score60Percent = (int)Math.Ceiling(maxPossibleScore * 0.60);

            int correct90Percent = (int)Math.Ceiling(maxPossibleCorrectAnswers * 0.90);
            int correct75Percent = (int)Math.Ceiling(maxPossibleCorrectAnswers * 0.75);
            int correct60Percent = (int)Math.Ceiling(maxPossibleCorrectAnswers * 0.60);

            List<Grade> defaultGrades = new List<Grade>
            {
                new Grade { Title = "5", MinScore = score90Percent, MinCorrectAnswers = correct90Percent, FailPenalty = maxPossibleScore, FailIncorrectAnswers = maxPossibleCorrectAnswers, IsPassingGrade = true },
                new Grade { Title = "4", MinScore = score75Percent, MinCorrectAnswers = correct75Percent, FailPenalty = maxPossibleScore, FailIncorrectAnswers = maxPossibleCorrectAnswers, IsPassingGrade = true },
                new Grade { Title = "3", MinScore = score60Percent, MinCorrectAnswers = correct60Percent, FailPenalty = maxPossibleScore, FailIncorrectAnswers = maxPossibleCorrectAnswers, IsPassingGrade = true },
                new Grade { Title = "2", MinScore = 0, MinCorrectAnswers = 0, FailPenalty = maxPossibleScore, FailIncorrectAnswers = maxPossibleCorrectAnswers, IsPassingGrade = false }
            };

            // Assign the newly created grades, ordered by MinScore descending
            Grades = defaultGrades.OrderByDescending(g => g.MinScore).ToArray();

            Debug.WriteLine($"\nDefault grades initialized based on Max Possible Score: {maxPossibleScore} and Max Possible Correct Answers: {maxPossibleCorrectAnswers}");
            foreach (var grade in Grades)
            {
                Debug.WriteLine($"- {grade.Title}: MinScore={grade.MinScore}, MinCorrectAnswers={grade.MinCorrectAnswers}, FailPenalty={grade.FailPenalty}, FailIncorrectAnswers={grade.FailIncorrectAnswers}, Passing={grade.IsPassingGrade}");
            }
        }
        public QuizResults GradeUserAnswers()
        {
            int userTotalScore = 0;
            int userCorrectAnswersCount = 0;
            int userIncorrectAnswersCount = 0;
            int userTotalPenalty = 0;

            if (Questions == null)
            {
                return new QuizResults();
            }

            foreach (var question in Questions)
            {
                int questionScore = 0;
                int questionCorrectCount = 0;
                int questionIncorrectCount = 0;
                int questionPenalty = 0;

                if (question.Answers == null) continue;

                switch (question.QuestionType)
                {
                    case questionType.radioButton:
                        // For radio buttons, only one answer can be selected.
                        var selectedRbAnswer = question.Answers.FirstOrDefault(a => a.IsUserSelected);
                        if (selectedRbAnswer != null)
                        {
                            if (selectedRbAnswer.IsTrue)
                            {
                                questionScore += selectedRbAnswer.Score;
                                questionCorrectCount++;
                            }
                            else
                            {
                                questionScore += selectedRbAnswer.Score; // Apply penalty if score is negative
                                questionIncorrectCount++;
                                questionPenalty += selectedRbAnswer.Penalty;
                            }
                        }
                        break;

                    case questionType.checkBox:
                        // For checkboxes, iterate through all answers to check selections.
                        foreach (var answer in question.Answers)
                        {
                            if (answer.IsUserSelected)
                            {
                                if (answer.IsTrue)
                                {
                                    questionScore += answer.Score;
                                    questionCorrectCount++;
                                }
                                else
                                {
                                    questionScore += answer.Score;
                                    questionIncorrectCount++;
                                    questionPenalty += answer.Penalty;
                                }
                            }
                        }

                        // VOIDING LOGIC FOR CHECKBOXES: If any incorrect answers were chosen, void the award score.
                        if (questionIncorrectCount > 0)
                        {
                            Debug.WriteLine($"  (Question: '{question.Content}') Incorrect answers were selected. Award score for this question is voided.");
                            // If the questionScore is positive (meaning awards outweighed penalties), set it to 0.
                            // If it's already negative (penalties outweighed awards), keep it negative.
                            if (questionScore > 0)
                            {
                                questionScore = 0;
                            }
                            questionCorrectCount = 0; // No correct answers are counted towards the total if incorrect ones were selected
                        }
                        break;

                    case questionType.textBox:
                        var correctTextBoxAnswer = question.Answers.FirstOrDefault(a => a.IsTrue);
                        var selectedTextBoxAnswer = question.Answers.FirstOrDefault(a => a.IsUserSelected);

                        if (correctTextBoxAnswer != null && selectedTextBoxAnswer != null &&
                            selectedTextBoxAnswer.Content.Equals(correctTextBoxAnswer.Content, StringComparison.OrdinalIgnoreCase))
                        {
                            questionScore += correctTextBoxAnswer.Score;
                            questionCorrectCount++;
                        }
                        else
                        {
                            if (question.Answers.Any(a => a.Score < 0))
                            {
                                questionScore += question.Answers.Where(a => a.Score < 0).Sum(a => a.Score);
                                questionPenalty += question.Answers.Where(a => a.Score < 0).Sum(a => a.Penalty);
                            }
                            questionIncorrectCount++;
                        }
                        break;
                }
                userTotalScore += questionScore;
                userCorrectAnswersCount += questionCorrectCount;
                userIncorrectAnswersCount += questionIncorrectCount;
                userTotalPenalty += questionPenalty;
            }

            if (Grades != null)
            {
                Grade finalGrade = Grades.FirstOrDefault(g =>
                userTotalScore >= g.MinScore &&
                userCorrectAnswersCount >= g.MinCorrectAnswers &&
                userTotalPenalty <= g.FailPenalty &&
                userIncorrectAnswersCount <= g.FailIncorrectAnswers
                );

                return new QuizResults
                {
                    FinalGrade = finalGrade,
                    TotalScore = userTotalScore,
                    CorrectAnswersCount = userCorrectAnswersCount,
                    IncorrectAnswersCount = userIncorrectAnswersCount,
                    TotalPenalty = userTotalPenalty
                };
            }
            else throw new ArgumentException("Grade cannot be null", nameof(Grades));
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n--- Starting a New Questionnaire with 10 Questions ---");

            // Define questions using DefaultAward (1) and DefaultPenalty (0) for scores where applicable.
            // Q1: Radio Button
            Question q1 = new Question
            {
                QuestionType = questionType.radioButton,
                Content = "What is the capital of Japan?",
                Answers = new Answer[]
                {
                    new Answer { Content = "Beijing", IsTrue = false, Score = 0 },
                    new Answer { Content = "Tokyo", IsTrue = true, Score = 1 }, 
                    new Answer { Content = "Seoul", IsTrue = false, Score = 0 }
                }
            };

            // Q2: Check Box
            Question q2 = new Question
            {
                QuestionType = questionType.checkBox,
                Content = "Which of these are programming languages? (Select all that apply)",
                Answers = new Answer[]
                {
                    new Answer { Content = "Python", IsTrue = true, Score = 1 },
                    new Answer { Content = "HTML", IsTrue = false, Score = 0 },
                    new Answer { Content = "Java", IsTrue = true, Score = 1 },
                    new Answer { Content = "CSS", IsTrue = false, Score = 0 }
                }
            };

            // Q3: Text Box
            Question q3 = new Question
            {
                QuestionType = questionType.textBox,
                Content = "What is the largest ocean on Earth?",
                Answers = new Answer[] { new Answer { Content = "Pacific", IsTrue = true, Score = 1 } }
            };

            // Q4: Radio Button
            Question q4 = new Question
            {
                QuestionType = questionType.radioButton,
                Content = "How many continents are there?",
                Answers = new Answer[]
                {
                    new Answer { Content = "5", IsTrue = false, Score = 0 },
                    new Answer { Content = "6", IsTrue = false, Score = 0 },
                    new Answer { Content = "7", IsTrue = true, Score = 1 } 
                }
            };

            // Q5: Check Box
            Question q5 = new Question
            {
                QuestionType = questionType.checkBox,
                Content = "Which of these are mammals? (Select all that apply)",
                Answers = new Answer[]
                {
                    new Answer { Content = "Whale", IsTrue = true, Score = 1 },
                    new Answer { Content = "Shark", IsTrue = false, Score = 0 },
                    new Answer { Content = "Bat", IsTrue = true, Score = 1 },
                    new Answer { Content = "Penguin", IsTrue = false, Score = 0 }
                }
            };

            // Q6: Text Box
            Question q6 = new Question
            {
                QuestionType = questionType.textBox,
                Content = "What is the chemical symbol for water?",
                Answers = new Answer[] { new Answer { Content = "H2O", IsTrue = true, Score = 1 } } // DefaultAward
            };

            // Q7: Radio Button
            Question q7 = new Question
            {
                QuestionType = questionType.radioButton,
                Content = "Which planet is known as the 'Red Planet'?",
                Answers = new Answer[]
                {
                    new Answer { Content = "Mars", IsTrue = true, Score = 1 },
                    new Answer { Content = "Jupiter", IsTrue = false, Score = 0 },
                    new Answer { Content = "Venus", IsTrue = false, Score = 0 }
                }
            };

            // Q8: Check Box
            Question q8 = new Question
            {
                QuestionType = questionType.checkBox,
                Content = "Which of these are primary colors? (Select all that apply)",
                Answers = new Answer[]
                {
                    new Answer { Content = "Red", IsTrue = true, Score = 1 },
                    new Answer { Content = "Green", IsTrue = false, Score = 0 },
                    new Answer { Content = "Blue", IsTrue = true, Score = 1 },
                    new Answer { Content = "Yellow", IsTrue = true, Score = 1 }
                }
            };

            // Q9: Text Box
            Question q9 = new Question
            {
                QuestionType = questionType.textBox,
                Content = "What is the fastest land animal?",
                Answers = new Answer[] { new Answer { Content = "Cheetah", IsTrue = true, Score = 1 } } // DefaultAward
            };

            // Q10: Radio Button
            Question q10 = new Question
            {
                QuestionType = questionType.radioButton,
                Content = "Is the Earth flat?",
                Answers = new Answer[]
                {
                    new Answer { Content = "Yes", IsTrue = false, Score = 0 },
                    new Answer { Content = "No", IsTrue = true, Score = 1 }
                }
            };
            Questionnaire myQuiz = new Questionnaire
            {
                Questions = new Question[] { q1, q2, q3, q4, q5, q6, q7, q8, q9, q10 },
            };

            myQuiz.SetDefaultPercentageGrades();

            Console.WriteLine("\n--- Begin Quiz ---\n");

            // Loop through questions for user input
            for (int i = 0; i < myQuiz.Questions.Length; i++)
            {
                Question currentQuestion = myQuiz.Questions[i];
                Console.WriteLine($"\nQuestion {i + 1}: {currentQuestion.Content}");

                // Reset IsUserSelected for all answers before getting input for the current question
                if (currentQuestion.Answers != null)
                {
                    foreach (var answer in currentQuestion.Answers)
                    {
                        answer.IsUserSelected = false;
                    }
                }
                switch (currentQuestion.QuestionType)
                {
                    case questionType.radioButton:
                        for (int j = 0; j < currentQuestion.Answers.Length; j++)
                        {
                            Console.WriteLine($"{j + 1}. {currentQuestion.Answers[j].Content}");
                        }
                        Console.Write("Enter your choice (number): ");
                        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= currentQuestion.Answers.Length)
                        {
                            currentQuestion.Answers[choice - 1].IsUserSelected = true;
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. No answer selected for this question.");
                        }
                        break;

                    case questionType.checkBox:
                        for (int j = 0; j < currentQuestion.Answers.Length; j++)
                        {
                            Console.WriteLine($"{j + 1}. {currentQuestion.Answers[j].Content}");
                        }
                        Console.Write("Enter your choices (comma-separated numbers, e.g., 1,3): ");
                        string input = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            List<int> userChoices = input.Split(',')
                                                         .Select(s => s.Trim())
                                                         .Where(s => int.TryParse(s, out _))
                                                         .Select(int.Parse)
                                                         .ToList();
                            foreach (int choiceNum in userChoices)
                            {
                                if (choiceNum >= 1 && choiceNum <= currentQuestion.Answers.Length)
                                {
                                    currentQuestion.Answers[choiceNum - 1].IsUserSelected = true;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No choices entered for this question.");
                        }
                        break;

                    case questionType.textBox:
                        Console.Write("Your answer: ");
                        string userAnswerText = Console.ReadLine();
                        // For text box, we'll ensure there's an Answer object to hold the user's input.
                        // If there are no predefined answers, we create one.
                        // If there are predefined answers (e.g., a correct one for comparison),
                        // we add the user's input as a new selected answer.
                        // The grading logic will then compare this selected answer's content.
                        if (currentQuestion.Answers == null || !currentQuestion.Answers.Any(a => a.IsUserSelected))
                        {
                            // If no answers or no user-selected answer yet, create one for user input
                            currentQuestion.Answers = (currentQuestion.Answers ?? Array.Empty<Answer>())
                                .Append(new Answer { Content = userAnswerText, IsUserSelected = true, Score = 0 })
                                .ToArray();
                        }
                        else
                        {
                            var existingSelected = currentQuestion.Answers.FirstOrDefault(a => a.IsUserSelected);
                            if (existingSelected != null)
                            {
                                existingSelected.Content = userAnswerText;
                            }
                            else
                            {
                                currentQuestion.Answers = currentQuestion.Answers.Append(new Answer { Content = userAnswerText, IsUserSelected = true, Score = 0 }).ToArray();
                            }
                        }
                        break;
                }
                Console.WriteLine("This question Score is: ", Convert.ToString(currentQuestion.Score));
            }

            Console.WriteLine("\n--- Quiz Finished ---");

            // Grade the user's answers using the Questionnaire's method
            QuizResults results = myQuiz.GradeUserAnswers();

            Console.WriteLine($"\nYour Final Score: {results.TotalScore}");
            Console.WriteLine($"Correct Answers: {results.CorrectAnswersCount}");
            Console.WriteLine($"Incorrect Answers: {results.IncorrectAnswersCount}");
            Console.WriteLine($"Total Penalty Applied: {results.TotalPenalty}");

            if (results.FinalGrade != null)
            {
                Console.WriteLine($"\nYour Grade: {results.FinalGrade.Title}");
                if (results.FinalGrade.IsPassingGrade)
                {
                    Console.WriteLine("Congratulations! You passed the quiz.");
                }
                else
                {
                    Console.WriteLine("You did not pass the quiz.");
                }
            }
            else
            {
                Console.WriteLine("\nCould not determine a grade based on your performance.");
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();

        }
    }
}
