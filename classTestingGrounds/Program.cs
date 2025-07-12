using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classTestingGrounds
{
    public enum questionType { textBox, checkBox, radioButton }
    internal class Program
    {
        class Answer
        {
            public string Content { get; set; }
            public int Score { get; set; }
            public bool IsTrue {  get; set; }
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
            public string Content { get; set; }
            public Answer[] Answers { get; set; }
            public virtual int Score
            {
                get
                {
                    return Answers?.Sum(answer => answer.Score) ?? 0;
                }
            }
            public virtual questionType QuestionType
            {
                get; set;
                //get
                //{
                //    if (this.Answers.Length == 0) { return questionType.textBox; }
                //    else
                //    {
                //        int trueAnswerCount = 0;
                //        foreach (Answer answer in Answers) { if (answer.IsTrue) { trueAnswerCount++; } }
                        //If there are only true answers, define as textBox
                //        if (this.Answers.Length == trueAnswerCount) { return questionType.textBox; }
                        //If there is only one true answer, define as radioButton
                //        else if (trueAnswerCount == 1) { return questionType.radioButton; }
                        //If there are multiple correct answers with any incorrect ones, define as checkBox
                //        else { return questionType.checkBox; }
                //    }
            }
        }
        class Grade
        {
            public string Title { get; set; }
            public int MinScore { get; set; }
            public int MinCorrectAnswers { get; set; }
            public int FailPenalty { get; set; } //Penalty points to fail the grade
            public int FailIncorrectAnswers { get; set; } //Number of wrong answers to fail the grade
            public bool IsPassingGrade {  get; set; }
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

                Console.WriteLine($"\nDefault grades initialized based on Max Possible Score: {maxPossibleScore} and Max Possible Correct Answers: {maxPossibleCorrectAnswers}");
                foreach (var grade in Grades)
                {
                    Console.WriteLine($"- {grade.Title}: MinScore={grade.MinScore}, MinCorrectAnswers={grade.MinCorrectAnswers}, FailPenalty={grade.FailPenalty}, FailIncorrectAnswers={grade.FailIncorrectAnswers}, Passing={grade.IsPassingGrade}");
                }
            }
        }
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
                    new Answer { Content = "Tokyo", IsTrue = true, Score = 1 }, // DefaultAward
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
                    new Answer { Content = "Python", IsTrue = true, Score = 1 }, // DefaultAward
                    new Answer { Content = "HTML", IsTrue = false, Score = 0 },
                    new Answer { Content = "Java", IsTrue = true, Score = 1 }, // DefaultAward
                    new Answer { Content = "CSS", IsTrue = false, Score = 0 }
                }
            };

            // Q3: Text Box
            Question q3 = new Question
            {
                QuestionType = questionType.textBox,
                Content = "What is the largest ocean on Earth?",
                Answers = new Answer[] { new Answer { Content = "Pacific", IsTrue = true, Score = 1 } } // DefaultAward
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
                    new Answer { Content = "7", IsTrue = true, Score = 1 } // DefaultAward
                }
            };

            // Q5: Check Box
            Question q5 = new Question
            {
                QuestionType = questionType.checkBox,
                Content = "Which of these are mammals? (Select all that apply)",
                Answers = new Answer[]
                {
                    new Answer { Content = "Whale", IsTrue = true, Score = 1 }, // DefaultAward
                    new Answer { Content = "Shark", IsTrue = false, Score = 0 },
                    new Answer { Content = "Bat", IsTrue = true, Score = 1 }, // DefaultAward
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
                    new Answer { Content = "Mars", IsTrue = true, Score = 1 }, // DefaultAward
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
                    new Answer { Content = "Red", IsTrue = true, Score = 1 }, // DefaultAward
                    new Answer { Content = "Green", IsTrue = false, Score = 0 },
                    new Answer { Content = "Blue", IsTrue = true, Score = 1 }, // DefaultAward
                    new Answer { Content = "Yellow", IsTrue = true, Score = 1 } // DefaultAward
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
                    new Answer { Content = "No", IsTrue = true, Score = 1 } // DefaultAward
                }
            };


            // Create a questionnaire with the 10 questions
            Questionnaire myQuiz = new Questionnaire
            {
                Questions = new Question[] { q1, q2, q3, q4, q5, q6, q7, q8, q9, q10 },
                // Grades array is initially empty or null, so SetDefaultPercentageGrades will populate it.
            };

            // Set default grades based on the questions added to the questionnaire
            myQuiz.SetDefaultPercentageGrades();

            int userTotalScore = 0;
            int userCorrectAnswersCount = 0;
            int userIncorrectAnswersCount = 0;
            int userTotalPenalty = 0;

            Console.WriteLine("\n--- Begin Quiz ---\n");

            for (int i = 0; i < myQuiz.Questions.Length; i++)
            {
                Question currentQuestion = myQuiz.Questions[i];
                Console.WriteLine($"\nQuestion {i + 1}: {currentQuestion.Content}");

                switch (currentQuestion.QuestionType)
                {
                    case questionType.radioButton:
                        // Display answers with numbers
                        for (int j = 0; j < currentQuestion.Answers.Length; j++)
                        {
                            Console.WriteLine($"{j + 1}. {currentQuestion.Answers[j].Content}");
                        }
                        Console.Write("Enter your choice (number): ");
                        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= currentQuestion.Answers.Length)
                        {
                            Answer selectedAnswer = currentQuestion.Answers[choice - 1];
                            if (selectedAnswer.IsTrue)
                            {
                                userTotalScore += selectedAnswer.Score;
                                userCorrectAnswersCount++;
                                Console.WriteLine("Correct!");
                            }
                            else
                            {
                                userTotalScore += selectedAnswer.Score; // Apply penalty if score is negative
                                userIncorrectAnswersCount++;
                                userTotalPenalty += selectedAnswer.Penalty;
                                Console.WriteLine("Incorrect.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. No score awarded/deducted for this question.");
                        }
                        break;

                    case questionType.checkBox:
                        // Display answers with numbers
                        for (int j = 0; j < currentQuestion.Answers.Length; j++)
                        {
                            Console.WriteLine($"{j + 1}. {currentQuestion.Answers[j].Content}");
                        }
                        Console.Write("Enter your choices (comma-separated numbers, e.g., 1,3): ");
                        string input = Console.ReadLine();
                        List<int> userChoices = new List<int>();
                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            userChoices = input.Split(',')
                                               .Select(s => s.Trim())
                                               .Where(s => int.TryParse(s, out _))
                                               .Select(int.Parse)
                                               .ToList();
                        }

                        int questionScore = 0;
                        int questionCorrectCount = 0;
                        int questionIncorrectCount = 0;
                        int questionPenalty = 0;

                        // Check user's selected answers
                        foreach (int choiceNum in userChoices)
                        {
                            if (choiceNum >= 1 && choiceNum <= currentQuestion.Answers.Length)
                            {
                                Answer selectedAnswer = currentQuestion.Answers[choiceNum - 1];
                                if (selectedAnswer.IsTrue)
                                {
                                    questionScore += selectedAnswer.Score;
                                    questionCorrectCount++;
                                }
                                else
                                {
                                    questionScore += selectedAnswer.Score; // Apply penalty if score is negative
                                    questionIncorrectCount++;
                                    questionPenalty += selectedAnswer.Penalty;
                                }
                            }
                        }

                        // NEW LOGIC: Void award score if incorrect answers were chosen for checkbox questions
                        if (questionIncorrectCount > 0)
                        {
                            Console.WriteLine("Incorrect answers were selected. Award score for this question is voided.");
                            // If the questionScore is positive (meaning awards outweighed penalties), set it to 0.
                            // If it's already negative (penalties outweighed awards), keep it negative.
                            if (questionScore > 0)
                            {
                                questionScore = 0;
                            }
                            questionCorrectCount = 0; // No correct answers are counted towards the total if incorrect ones were selected
                        }

                        // Check for unselected true answers (missed correct answers)
                        foreach (var answer in currentQuestion.Answers)
                        {
                            // If an 'IsTrue' answer was not selected by the user AND the question wasn't already voided for incorrect choices
                            // (or if you want to still report missed correct answers even if voided)
                            if (answer.IsTrue && !userChoices.Contains(Array.IndexOf(currentQuestion.Answers, answer) + 1))
                            {
                                Console.WriteLine($"Missed correct answer: {answer.Content}");
                            }
                        }

                        if (questionCorrectCount > 0 || questionIncorrectCount > 0 || questionScore < 0) // Ensure score is added even if only penalties
                        {
                            userTotalScore += questionScore;
                            userCorrectAnswersCount += questionCorrectCount;
                            userIncorrectAnswersCount += questionIncorrectCount;
                            userTotalPenalty += questionPenalty;
                            Console.WriteLine($"Question score: {questionScore}");
                        }
                        else
                        {
                            Console.WriteLine("No valid choices entered for this question.");
                        }
                        break;

                    case questionType.textBox:
                        Console.Write("Your answer: ");
                        string userAnswerText = Console.ReadLine();
                        // For textBox, we'll assume a single 'IsTrue' answer for comparison and scoring.
                        if (currentQuestion.Answers != null && currentQuestion.Answers.Length > 0)
                        {
                            Answer correctAnswer = currentQuestion.Answers.FirstOrDefault(a => a.IsTrue);
                            if (correctAnswer != null && userAnswerText.Trim().Equals(correctAnswer.Content, StringComparison.OrdinalIgnoreCase))
                            {
                                userTotalScore += correctAnswer.Score;
                                userCorrectAnswersCount++;
                                Console.WriteLine("Correct!");
                            }
                            else
                            {
                                Console.WriteLine("Incorrect or not matching expected answer.");
                                // If a textBox has a penalty for incorrect text answers, apply it.
                                if (currentQuestion.Answers.Any(a => a.Score < 0))
                                {
                                    userTotalScore += currentQuestion.Answers.Where(a => a.Score < 0).Sum(a => a.Score);
                                    userTotalPenalty += currentQuestion.Answers.Where(a => a.Score < 0).Sum(a => a.Penalty);
                                }
                                userIncorrectAnswersCount++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("No predefined answer for this text box question. Input recorded.");
                        }
                        break;
                }
                Console.WriteLine($"Current Total Score: {userTotalScore}");
            }

            Console.WriteLine("\n--- Quiz Finished ---");
            Console.WriteLine($"\nYour Final Score: {userTotalScore}");
            Console.WriteLine($"Correct Answers: {userCorrectAnswersCount}");
            Console.WriteLine($"Incorrect Answers: {userIncorrectAnswersCount}");
            Console.WriteLine($"Total Penalty Applied: {userTotalPenalty}");

            // Determine the final grade using the percentage-based grades
            // Grades are already sorted descending by MinScore within SetDefaultPercentageGrades().
            // We find the FIRST grade that the user meets ALL criteria for.
            Grade finalGrade = myQuiz.Grades.FirstOrDefault(g =>
                userTotalScore >= g.MinScore &&
                userCorrectAnswersCount >= g.MinCorrectAnswers &&
                userTotalPenalty <= g.FailPenalty &&
                userIncorrectAnswersCount <= g.FailIncorrectAnswers
            );

            if (finalGrade != null)
            {
                Console.WriteLine($"\nYour Grade: {finalGrade.Title}");
                // The IsPassingGrade property is now correctly set in the Grade object itself.
                if (finalGrade.IsPassingGrade)
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
