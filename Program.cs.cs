using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    class Chess
    {
        // const string aToH = "ABCDEFGH";   ???
        static void Main(string[] args)
        {
            chessGame theGame = new chessGame();
            theGame.playChess();
        }
    }

    class chessGame
    {
        piece[] chessBoard;
        string[] fiftyLastBoards;
        bool isWhiteTurn;
        bool isGameOver = false;
        string currentMove;
        string prevMove = "";
        string fromSquare;
        string toSquare;
        bool isEnPassant = false;
        piece playedPiece;
        piece capturedPiece;
        string playingSideKingCurrentSquare = "E1";
        string waitingSideKingCurrentSquare = "E8";
        string whoIsThreateningOnTheKing = "";
        int movesWithoutCaptureOrPawnCounter = 0;
        string whiteRooksAndKingMovementStatus = "NNN"; // king, 'A'-rook, 'H'-rook
        string blackRooksAndKingMovementStatus = "NNN"; // king, 'A'-rook, 'H'-rook

        public chessGame()
        {
            isWhiteTurn = true;
            initBoard();
            Console.WriteLine(ToString());
            Console.WriteLine("White player, please enter your move");        
        }

        void initBoard()
        {
            chessBoard = new piece[64];
            fiftyLastBoards = new string[50];

            for (int i = 0; i < 8; i++)
            {
                chessBoard[i + 8] = new pawn(true, indexToSquare(i + 8));
                chessBoard[i + 48] = new pawn(false, indexToSquare(i + 48));
            }
            chessBoard[squareToIndex("A1")] = new rook(true, "A1");
            chessBoard[squareToIndex("H1")] = new rook(true, "H1");
            chessBoard[squareToIndex("A8")] = new rook(false, "A8");
            chessBoard[squareToIndex("H8")] = new rook(false, "H8");

            chessBoard[squareToIndex("C1")] = new bishop(true, "C1");
            chessBoard[squareToIndex("F1")] = new bishop(true, "F1");
            chessBoard[squareToIndex("C8")] = new bishop(false, "C8");
            chessBoard[squareToIndex("F8")] = new bishop(false, "F8");

            chessBoard[squareToIndex("B1")] = new knight(true, "B1");
            chessBoard[squareToIndex("G1")] = new knight(true, "G1");
            chessBoard[squareToIndex("B8")] = new knight(false, "B8");
            chessBoard[squareToIndex("G8")] = new knight(false, "G8");

            chessBoard[squareToIndex("D1")] = new queen(true, "D1");
            chessBoard[squareToIndex("D8")] = new queen(false, "D8");

            chessBoard[squareToIndex("E1")] = new king(true, "E1");
            chessBoard[squareToIndex("E8")] = new king(false, "E8");
        }

        public void playChess()
        { 
            while (!isGameOver)
            {
                currentMove = Console.ReadLine().Trim().ToUpper();
                if (inputIsValid())
                {
                    fromSquare = currentMove.Substring(0, 2);
                    toSquare = currentMove.Substring(2, 2);
                    playedPiece = chessBoard[squareToIndex(fromSquare)];
                    capturedPiece = chessBoard[squareToIndex(toSquare)];
                    isEnPassant = false;

                    if (moveIsBasicallyLegal())
                    {
                        if ((playedPiece is king) & (Math.Abs(fromSquare[0] - toSquare[0]) == 2))
                        {
                            if (isTheCastlingLegal() & playingSideAvoidsOwnCheck())
                            {
                                castle();
                                completePlayingMoveProcedure();
                            }
                            else
                                Console.WriteLine("Illegal castling - Your king is under chess or the square he will jump over when castling is under threat or " +
                                    "your king/rook had already moved.");
                        }
                        else if (!(playedPiece is pawn))
                            if (playedPiece.move(chessBoard).Contains(toSquare))
                                playMoveOnlyIfNotPuttingItselfUnderCheck();
                            else
                                Console.WriteLine("Illegal move - this piece cannot move to that square.");                                
                        else if (playedPiece is pawn)
                            if (((pawn)playedPiece).pawnsCapture(chessBoard, false).Contains(toSquare))
                            {
                                playMoveOnlyIfNotPuttingItselfUnderCheck();
                            }
                            else if ((playedPiece.move(chessBoard).Contains(toSquare)) & (capturedPiece == null))
                            {
                                playMoveOnlyIfNotPuttingItselfUnderCheck();
                            }
                            else if (((pawn)playedPiece).pawnsCapture(chessBoard, true).Contains(toSquare)
                                        & (isEnPassant = (capturedPiece == null) & (prevMove[1] == 'P') &
                                        (Math.Abs(int.Parse("" + prevMove[3]) - int.Parse("" + prevMove[5])) == 2) & (prevMove[2] == toSquare[0])))
                            {
                                playMoveOnlyIfNotPuttingItselfUnderCheck();
                            }
                            else
                                Console.WriteLine("Illegal move - this piece cannot move to that square.");
                    }
                    else
                        Console.WriteLine("The move is illegal - the first square must be occupied by a piece of your own color" +
                            "and the second square must be empty or occupied by a piece of the opponent color.");
                }
                else
                    Console.WriteLine("Input is not valid. Make sure you insert 4 characters, representing two legal squares.");
            }
        }

        bool inputIsValid()
        {
            return currentMove.Length == 4 && validSquare(currentMove.Substring(0, 2)) & validSquare(currentMove.Substring(2, 2));
        }

        bool moveIsBasicallyLegal()
        {
            return ((playedPiece != null) && (playedPiece.getColor() == isWhiteTurn)) && ((capturedPiece == null) || (capturedPiece.getColor() != isWhiteTurn));
        }

        bool playingSideAvoidsOwnCheck()
        {
            //string whichKingToCheck = optionalMoveCheck ? waitingSideKingCurrentSquare : playingSideKingCurrentSquare;
            if (pieceIsUnderThreat(chessBoard[squareToIndex(playingSideKingCurrentSquare)]).Length != 0)
            {
                Console.WriteLine("Illegal move - You are putting your king under chess. Please enter a different move and watch your king!");
                return false;
                //if (!optionalMoveCheck)
            }
            else
                return true;
        }
        
        bool playingSideResponsesProperlyToCounterCheck()
        {
            if ((whoIsThreateningOnTheKing.Length != 0) & (pieceIsUnderThreat(chessBoard[squareToIndex(playingSideKingCurrentSquare)]).Length != 0))
            {
                Console.WriteLine("Illegal move - Your king is still under chess. Please enter a different move and watch your king!");
                return false;
                //if (!optionalMoveCheck)   
            }
            return true;
        }

        void undoMove(piece capturedPieceContainer, string fromSquare, string toSquare)
        {
            chessBoard[squareToIndex(fromSquare)] = chessBoard[squareToIndex(toSquare)];
            chessBoard[squareToIndex(toSquare)] = capturedPieceContainer;
            chessBoard[squareToIndex(fromSquare)].setSquare(fromSquare);
            if (((chessBoard[squareToIndex(fromSquare)].ToString() == "BK") & (isWhiteTurn)) | ((chessBoard[squareToIndex(fromSquare)].ToString() == "WK") & (!isWhiteTurn)))
                waitingSideKingCurrentSquare = fromSquare;
            if (((chessBoard[squareToIndex(fromSquare)].ToString() == "BK") & (!isWhiteTurn)) | ((chessBoard[squareToIndex(fromSquare)].ToString() == "WK") & (isWhiteTurn)))
                playingSideKingCurrentSquare = fromSquare;
        }

        piece playMove(string fromSquare, string toSquare)
        {
            piece capturedPieceContainer = chessBoard[squareToIndex(toSquare)];
            chessBoard[squareToIndex(toSquare)] = chessBoard[squareToIndex(fromSquare)];
            chessBoard[squareToIndex(fromSquare)] = null;
            chessBoard[squareToIndex(toSquare)].setSquare(toSquare);
            if (((chessBoard[squareToIndex(toSquare)].ToString() == "BK") & (isWhiteTurn)) | ((chessBoard[squareToIndex(toSquare)].ToString() == "WK") & (!isWhiteTurn)))
                waitingSideKingCurrentSquare = toSquare;
            if (((chessBoard[squareToIndex(toSquare)].ToString() == "BK") & (!isWhiteTurn)) | ((chessBoard[squareToIndex(toSquare)].ToString() == "WK") & (isWhiteTurn)))
                playingSideKingCurrentSquare = toSquare;
            return capturedPieceContainer;
        }

        void completePlayingMoveProcedure()
        {
            updateDrawCheckingVariables();
            if ((isWhiteTurn & playedPiece is pawn & toSquare[1] == '8') | (!isWhiteTurn & playedPiece is pawn & toSquare[1] == '1'))
                promotePawn();
            if (playedPiece.ToString()[1] == 'R')
                updateRookMovementStatus();
            if (playedPiece.ToString()[1] == 'K')
            {
                ((king)playedPiece).madeMove = true;
                if (playedPiece.getColor())
                    whiteRooksAndKingMovementStatus = "Y" + whiteRooksAndKingMovementStatus.Substring(1, 2);
                else
                    blackRooksAndKingMovementStatus = "Y" + blackRooksAndKingMovementStatus.Substring(1, 2);
            }
            playedPiece.setSquare(toSquare);
            if (isEnPassant)
                chessBoard[squareToIndex(prevMove.Substring(4, 2))] = null;
            Console.WriteLine(ToString());
            if ((whoIsThreateningOnTheKing = pieceIsUnderThreat(chessBoard[squareToIndex(waitingSideKingCurrentSquare)])).Length != 0)
            {
                if (checkmate(whoIsThreateningOnTheKing))
                {
                    Console.WriteLine("checkmate.");
                    isGameOver = true;
                }
                else
                    Console.WriteLine("Chess! {0} player, your king is under chess. React properly. ", isWhiteTurn ? "Black" : "White");
            }
            else if (isStalmate(!isWhiteTurn))
            {
                Console.WriteLine("It's a stalmate, therefore it is a draw.");
                isGameOver = true;
            }
            string piecesRemain = piecesOnBoard(false, isWhiteTurn);

            if (piecesRemain.Length == 6 & ((piecesRemain.Contains("N")) | (piecesRemain.Contains("BB")) | (piecesRemain.Contains("WB"))))
            {
                Console.WriteLine("Checkmate is not possible to either side. therefore it is a draw.");
                isGameOver = true;
            }
            else if (!isGameOver & movesWithoutCaptureOrPawnCounter == 50)
            {
                Console.WriteLine("50 Moves rule - it is a draw.");
                isGameOver = true;
            }
            else if (checkThreefoldRepetition())
            {
                Console.WriteLine("Threefold repetition rule - it is a draw.");
                isGameOver = true;
            }
            isWhiteTurn = !isWhiteTurn;
            playingSideKingCurrentSquare += waitingSideKingCurrentSquare;
            waitingSideKingCurrentSquare = playingSideKingCurrentSquare.Substring(0, 2);
            playingSideKingCurrentSquare = playingSideKingCurrentSquare.Substring(2, 2);
            prevMove = playedPiece + currentMove;

            if (!isGameOver)
                Console.WriteLine((isWhiteTurn ? "White" : "Black") + " player, please enter your move");
        }

        void playMoveOnlyIfNotPuttingItselfUnderCheck()
        {
            piece capturedPieceContainer = playMove(fromSquare, toSquare);
            if (playingSideResponsesProperlyToCounterCheck() && playingSideAvoidsOwnCheck())
                completePlayingMoveProcedure();
            else
                undoMove(capturedPieceContainer,fromSquare,toSquare);
        }

        void updateDrawCheckingVariables()
            {
                if ((playedPiece is pawn) | (capturedPiece != null))
                {
                    movesWithoutCaptureOrPawnCounter = 0;
                    deleteSavedBoardStates();
                }
                else
                {
                    movesWithoutCaptureOrPawnCounter++;
                    saveBoardState();
                }
            }

        void promotePawn()
        {
            piece newPiece;
            Console.WriteLine("{0} player, you promot your pawn. chose what piece you want: Queen (enter Q), Knight (K), Bishop (B) or Rookromotion (R)", isWhiteTurn ? "White" : "Black");
            char input = char.Parse(Console.ReadLine().ToUpper());
            while (!"QKBR".Contains(input))
            {
                Console.WriteLine("Again, type 'Q' for queen, 'K' for knight, 'B' for bishop or 'R' for Rook");
                input = char.Parse(Console.ReadLine().ToUpper());
            }

            switch (input)
            {
                case ('Q'):
                    newPiece = new queen(isWhiteTurn, toSquare);
                    break;
                case ('K'):
                    newPiece = new knight(isWhiteTurn, toSquare);
                    break;
                case ('B'):
                    newPiece = new bishop(isWhiteTurn, toSquare);
                    break;
                default:
                    newPiece = new rook(isWhiteTurn, toSquare);
                    break;
            }
            chessBoard[squareToIndex(toSquare)] = newPiece;
            Console.WriteLine("Promotion succedded. Enjoy your new piece!");
        }

        void castle()
        {
            playingSideKingCurrentSquare = toSquare;
            chessBoard[squareToIndex(toSquare)] = chessBoard[squareToIndex(fromSquare)];
            chessBoard[squareToIndex(fromSquare)] = null;
            chessBoard[squareToIndex(toSquare)].setSquare(toSquare);
            if (fromSquare[0] - toSquare[0] == 2)
            {
                chessBoard[squareToIndex(toSquare) + 1] = chessBoard[squareToIndex(toSquare) - 2];
                chessBoard[squareToIndex(toSquare) - 2] = null;
                ((rook)chessBoard[squareToIndex(toSquare) + 1]).madeMove = true;
                ((rook)chessBoard[squareToIndex(toSquare) + 1]).setSquare(indexToSquare(squareToIndex(toSquare) + 1));
            }
            if (fromSquare[0] - toSquare[0] == -2)
            {
                chessBoard[squareToIndex(toSquare) - 1] = chessBoard[squareToIndex(toSquare) + 1];
                chessBoard[squareToIndex(toSquare) + 1] = null;
                ((rook)chessBoard[squareToIndex(toSquare) - 1]).madeMove = true;
                ((rook)chessBoard[squareToIndex(toSquare) - 1]).setSquare(indexToSquare(squareToIndex(toSquare) - 1));

            }
            updateRookMovementStatus();
        }

        bool isTheCastlingLegal()
        {
            bool isTheCastlingLegal = false;
            int i = fromSquare[0] - toSquare[0];
            int rookIndex = i == 2 ? squareToIndex(toSquare) - 2 : squareToIndex(toSquare) + 1;
            char squareInsideCastling = (char)(toSquare[0] + (i / 2));
            chessBoard[squareToIndex(toSquare) + (i / 2)] = new piece(isWhiteTurn, "" + squareInsideCastling + toSquare[1]);
            if ((whoIsThreateningOnTheKing.Length == 0) & (pieceIsUnderThreat(chessBoard[squareToIndex(toSquare) + (i / 2)]).Length == 0) & 
                !((king)playedPiece).madeMove & !((rook)chessBoard[rookIndex]).madeMove)
                isTheCastlingLegal = true;
            chessBoard[squareToIndex(toSquare) + (i / 2)] = null;
            return isTheCastlingLegal;
        }

         bool isOptionalMovePutsPlyaerUnderCheck(string fromSquare, string toSquare)
         {
             bool result = false;
             piece capturedPieceContainer = playMove(fromSquare, toSquare);
             if (pieceIsUnderThreat(chessBoard[squareToIndex(waitingSideKingCurrentSquare)]).Length != 0)
                 result = true;
            undoMove(capturedPieceContainer, fromSquare, toSquare);
             return result;
          }

        bool isStalmate(bool isWhite)
        {
            bool result = true;
            
            for (int i = 0; i < 64 & result; i++)
            {
                if ((chessBoard[i] != null) && (chessBoard[i].getColor() == isWhite))
                {
                    string moves = chessBoard[i].move(chessBoard);
                    if (chessBoard[i] is pawn)
                        moves += ((pawn)chessBoard[i]).pawnsCapture(chessBoard, false);
                    //What about pawn capture?? need to be added
                    for (int j = 0; j < moves.Length & result; j += 2)
                        result = isOptionalMovePutsPlyaerUnderCheck(indexToSquare(i), moves.Substring(j, 2));
                }                 
            }
            return result;
        }

        bool checkmate(string threatningSquares)
        {
            bool checkmateNotRefuted = true;
            piece relevantKing = chessBoard[squareToIndex(waitingSideKingCurrentSquare)];
            string kingsEscapeSquares = relevantKing.move(chessBoard);
            int i = 0;
            while (i < kingsEscapeSquares.Length & checkmateNotRefuted)
            {
                if (!isOptionalMovePutsPlyaerUnderCheck(waitingSideKingCurrentSquare, kingsEscapeSquares.Substring(i, 2)))
                    checkmateNotRefuted = false;
                i += 2;
            }
            if (threatningSquares.Length == 2 & checkmateNotRefuted)
            {
                string whoCanKillTheKingsAttacker = pieceIsUnderThreat(chessBoard[squareToIndex(threatningSquares)]);
                if ((whoCanKillTheKingsAttacker.Length > 0) & (whoCanKillTheKingsAttacker != relevantKing.getSquare()))
                {
                    checkmateNotRefuted = false;
                }
                else
                {
                    string threatningPieceName = chessBoard[squareToIndex(threatningSquares)].ToString();
                    if (!threatningPieceName.Contains("N") & !threatningPieceName.Contains("P"))
                    {
                        string squaresBetweenKingAndAttacker = squaresLineBetweenTwoSquares(waitingSideKingCurrentSquare, threatningSquares);
                        if (squaresBetweenKingAndAttacker != "")
                        {
                            i = 0;
                            string singleSquare;
                            while (i < squaresBetweenKingAndAttacker.Length & checkmateNotRefuted)
                            {
                                singleSquare = squaresBetweenKingAndAttacker.Substring(i, 2);
                                piece demoPiece = new piece(isWhiteTurn, singleSquare);
                                chessBoard[squareToIndex(singleSquare)] = demoPiece;
                                string whoCanBlock = pieceIsUnderThreat(chessBoard[squareToIndex(singleSquare)]);
                                int j = 0;
                                while (j < whoCanBlock.Length)
                                {
                                    if (chessBoard[squareToIndex(whoCanBlock.Substring(j, 2))] is pawn)
                                        whoCanBlock = whoCanBlock.Remove(j, 2);
                                    else
                                        j += 2;
                                }
                                // The code miss here the scenario where a pawn can block by moving (not by capturing)
                                if ((whoCanBlock.Length != 0) & ((whoCanBlock.Length != 2) | (!whoCanBlock.Contains(waitingSideKingCurrentSquare))))

                                    checkmateNotRefuted = false;
                                chessBoard[squareToIndex(singleSquare)] = null;
                                i += 2;
                            }
                        }
                    }
                }
            }

                return checkmateNotRefuted;
        }

        string squaresLineBetweenTwoSquares(string s1, string s2)
        {
            string result = "";
            int higherNumber = s1[1] < s2[1] ? int.Parse("" + s2[1]) : int.Parse("" + s1[1]);
            char higherLetter = s1[0] < s2[0] ? s2[0] : s1[0];
            int lowerNumber = s1[1] > s2[1] ? int.Parse("" + s2[1]) : int.Parse("" + s1[1]);
            char lowerLetter = s1[0] > s2[0] ? s2[0] : s1[0];

            if (higherNumber == lowerNumber)
                while (higherLetter - lowerLetter > 1)
                {
                    lowerLetter++;
                    result += ("" + lowerLetter) + lowerNumber;
                }
            else if (higherLetter == lowerLetter)
                while (higherNumber - lowerNumber > 1)
                {
                    lowerNumber++;
                    result += ("" + lowerLetter) + lowerNumber;
                }
            else if (higherNumber - lowerNumber == higherLetter - lowerLetter)
                while (higherNumber - lowerNumber > 1)
                {
                    lowerNumber++;
                    lowerLetter++;
                    result += ("" + lowerLetter) + lowerNumber;
                }
            else
                result = "No direct line";
            return result;
        }

        string pieceIsUnderThreat(piece thePiece)
        {
            string result = "";
            char opponentColor = thePiece.getColor() ? 'B' : 'W';

            string knightThreatning = thePiece.knightMoves();
            int i = 0;
            string singleSquare;
            while (i < knightThreatning.Length)
                {
                singleSquare = knightThreatning.Substring(i, 2);
                if (validSquare(singleSquare) && !(chessBoard[squareToIndex(singleSquare)] is null))
                {
                    if (chessBoard[squareToIndex(singleSquare)].ToString() == "" + opponentColor + 'N')
                        result += singleSquare;
                }
                i += 2;
                }

            string pawnThreatning = thePiece.pawnsCapture(chessBoard, false);
            i = 0;
            while (i < pawnThreatning.Length)
            {
                singleSquare = pawnThreatning.Substring(i, 2);
                if (validSquare(singleSquare) && !(chessBoard[squareToIndex(singleSquare)] is null))
                {
                    if (chessBoard[squareToIndex(singleSquare)].ToString() == "" + opponentColor + 'P')
                        result += singleSquare;
                }
                i += 2;
            }
            string kingsThreatning = thePiece.checkPath(chessBoard, true, true) + thePiece.checkPath(chessBoard, false, true);
            
            i = 0;
            while (i < kingsThreatning.Length)
            {
                singleSquare = kingsThreatning.Substring(i, 2);
                if (validSquare(singleSquare) && !(chessBoard[squareToIndex(singleSquare)] is null))
                {
                    if (chessBoard[squareToIndex(singleSquare)].ToString() == "" + opponentColor + 'K')
                        result += singleSquare;
                }
                i += 2;
            }

            string diagonalThreatning = thePiece.checkPath(chessBoard, true, false);
            i = 0;
            while (i < diagonalThreatning.Length)
            {
                singleSquare = diagonalThreatning.Substring(i, 2);
                if (validSquare(singleSquare) && !(chessBoard[squareToIndex(singleSquare)] is null))
                {
                    string nameOfCheckedPiece = chessBoard[squareToIndex(singleSquare)].ToString();
                    if ((nameOfCheckedPiece == "" + opponentColor + 'Q') | (nameOfCheckedPiece == "" + opponentColor + 'B'))
                        result += singleSquare;
                }
                i += 2;
            }
            string straightThreatning = thePiece.checkPath(chessBoard, false, false);
            i = 0;
            while (i < straightThreatning.Length)
            {
                singleSquare = straightThreatning.Substring(i, 2);
                if (validSquare(singleSquare) && !(chessBoard[squareToIndex(singleSquare)] is null))
                {
                    string nameOfCheckedPiece = chessBoard[squareToIndex(singleSquare)].ToString();
                    if ((nameOfCheckedPiece == "" + opponentColor + 'Q') | (nameOfCheckedPiece == "" + opponentColor + 'R'))
                        result += singleSquare;
                }
                i += 2;
            }
            return result;
        }

        public static bool validSquare(string square)
        {
            if ((square.Length == 2) && ("ABCDEFGH".Contains(square[0])) & ("12345678".Contains(square[1])))
                return true;
            return false;
        }

        public static string indexToSquare(int index)
        {
            if (index < 0 | index > 63)
                return "not valid index";
            int digit = index / 8 + 1;
            int letter = index % 8;
            return "" + "ABCDEFGH"[letter] + digit;
        }

        public static int squareToIndex(string square)
        {
            if (!validSquare(square))
                return -1;

            return   (int.Parse("" + square[1]) - 1) * 8 + "ABCDEFGH".IndexOf(square[0]);
        }

        public string piecesOnBoard(bool withEmptySquares, bool isWhiteTurn)
        {
            string result = "";
            if (withEmptySquares)
            {
                result = isWhiteTurn ? "WT" : "BT";
                result += whiteRooksAndKingMovementStatus + blackRooksAndKingMovementStatus;
            }
            string EE = withEmptySquares ? "EE" : "";
            for (int i = 0; i < 64; i++)
            {
                result += chessBoard[i] == null ? EE : chessBoard[i].ToString();
            }
            return result;
        }

        public void saveBoardState()
        {
            int i = 0;
            for (; i < 50 & fiftyLastBoards[i] != null; i++) { }
            fiftyLastBoards[i] = piecesOnBoard(true, isWhiteTurn);
        }

        public bool checkThreefoldRepetition()
        {
            int identicalPositionCounter = 0;
            for (int i = 0; i < 50 & fiftyLastBoards[i] != null & identicalPositionCounter < 2; i++)
            {
                identicalPositionCounter = 0;
                for (int j = i + 1; j < 50 & fiftyLastBoards[j] != null; j++)
                {
                    if (fiftyLastBoards[i] == fiftyLastBoards[j])
                        identicalPositionCounter++;
                }
            }
            if (identicalPositionCounter == 2)
                return true;
            return false;
        }

        public void deleteSavedBoardStates()
        {
            for (int i = 0; i < 50 & fiftyLastBoards[i] != null; i++)
            {
                fiftyLastBoards[i] = null;
            }
        }

        void updateRookMovementStatus()
        {
            ((rook)playedPiece).madeMove = true;
            if (playedPiece.getColor())
                if (fromSquare[0] == 'A')
                    whiteRooksAndKingMovementStatus = whiteRooksAndKingMovementStatus.Substring(0, 1) + 'Y' + whiteRooksAndKingMovementStatus.Substring(2, 1);
                else if (fromSquare[0] == 'H')
                    whiteRooksAndKingMovementStatus = whiteRooksAndKingMovementStatus.Substring(0, 2) + 'Y';
                else
                if (fromSquare[0] == 'A')
                    blackRooksAndKingMovementStatus = blackRooksAndKingMovementStatus.Substring(0, 1) + 'Y' + blackRooksAndKingMovementStatus.Substring(2, 1);
                else if (fromSquare[0] == 'H')
                    blackRooksAndKingMovementStatus = blackRooksAndKingMovementStatus.Substring(0, 2) + 'Y';
        }

        public override string ToString()
        {
            string currentBoard = "";
            currentBoard = "      A     B     C     D     E     F     G     H\n    ________________________________________________\n";
            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                    currentBoard += "   |     |     |     |     |     |     |     |     |\n" + (i / 8 + 1) + "  |";
                currentBoard += chessBoard[i] == null ? "     |" : " " + chessBoard[i] + "  |";
                if ((i + 1) % 8 == 0)
                    currentBoard += " " + (i / 8 + 1) + "\n   |_____|_____|_____|_____|_____|_____|_____|_____|\n";
            }
            currentBoard += "\n      A     B     C     D     E     F     G     H\n";
            return currentBoard;
        }

    }

    class piece
    {
        protected bool isWhite;
        protected string currentSquare;
        protected string aToH = "ABCDEFGH";
        public piece(bool isWhite, string square)
        {
            setColor(isWhite);
            setSquare(square);
        }

        public void setColor(bool isWhite)
        {
            this.isWhite = isWhite;
        }
        public bool getColor()
        {
            return isWhite;
        }
        public bool setSquare(string square)
        {
            if (chessGame.validSquare(square))
            {
                currentSquare = square.Trim().ToUpper();
                return true;
            }
            return false;
        }
        public string getSquare()
        {
            return currentSquare;
        }

        public virtual string move(piece[] theBoard)
        {
            return "";
        }

        public string checkPath(piece[] theBoard, bool diagonally, bool itIsKing)  
            // maybe logically not right to write this method under "piece" since knight aren't using it
        {
            string path = "";
            bool block = false;
            bool oneSquareLimit;
            int direction = diagonally ? 9 : 1;
            int myIndex = chessGame.squareToIndex(currentSquare);
            int runningIndex = myIndex;
            //to the right side
            for (int i = 0; i < 4; i++)
            {
                oneSquareLimit = false;
                while (chessGame.validSquare(chessGame.indexToSquare(runningIndex + direction)) & !block & !oneSquareLimit)
                {
                    char deadEnd = chessGame.indexToSquare(runningIndex)[0];
                    if (theBoard[runningIndex + direction] == null)
                    {
                        if (((deadEnd == 'A') & (direction == -1 | direction == 7 | direction == -9)) |
                            ((deadEnd == 'H') & (direction == 1 | direction == -7 | direction == 9)))
                                block = true;
                        else
                            path += chessGame.indexToSquare(runningIndex + direction);
                        runningIndex += direction;
                    }
                    else
                    {
                        block = true;
                        if ((!(((deadEnd == 'A') & (direction == -1 | direction == 7 | direction == -9)) |
                                  ((deadEnd == 'H') & (direction == 1 | direction == -7 | direction == 9))))
                            & (theBoard[runningIndex + direction].isWhite != isWhite))
                                 path += chessGame.indexToSquare(runningIndex + direction);
                    }
                    if (itIsKing)
                        oneSquareLimit = true;
                }
                if (direction > 0)
                    direction *= -1;
                else
                    direction = diagonally ? 7 : 8;
                runningIndex = myIndex;
                block = false;
            }
            // Console.WriteLine("the path is  " + path + "  and my name is " + this + "  " + currentSquare);
                return path;
        }

        public string knightMoves()
        {
            string result = "";
            int letter = aToH.IndexOf(currentSquare[0]);
            int digit = int.Parse("" + currentSquare[1]);
            for (int i = -2; i <= 2; i += 4)
                for (int j = -1; j <= 1; j += 2)
                {
                    if ((letter + i >= 0) & (letter + i <= 7) & (digit + j >= 1) & (digit + j <= 8))
                        result += "" + aToH[letter + i] + (digit + j);
                    if ((letter + j >= 0) & (letter + j <= 7) & (digit + i >= 1) & (digit + i <= 8))
                        result += "" + aToH[letter + j] + (digit + i);
                }
            return result;
        }

        public string pawnsCapture(piece[] theBoard, bool checkingEnPassantOption)
        {
            string possibleCaptures = "";
            int x = isWhite ? 1 : -1;
            if (getSquare()[0] != 'A')
            {
                if ((isWhite & int.Parse(""+getSquare()[1]) < 7) | (!isWhite & int.Parse(""+getSquare()[1]) > 2))
                {
                    piece optionalCapture = theBoard[chessGame.squareToIndex("" + aToH[aToH.IndexOf(getSquare()[0]) - 1] + (int.Parse("" + getSquare()[1]) + x))];
                    if ((optionalCapture != null) && (optionalCapture.getColor() != isWhite))
                        possibleCaptures += optionalCapture.getSquare();
                    else if (checkingEnPassantOption & optionalCapture == null)
                        possibleCaptures += "" + aToH[aToH.IndexOf(getSquare()[0]) - 1] + (int.Parse("" + getSquare()[1]) + x);
                }
            }
            if (getSquare()[0] != 'H')
            {
                if ((isWhite & int.Parse("" + getSquare()[1]) < 7) | (!isWhite & int.Parse("" + getSquare()[1]) > 2))
                {
                    piece optionalCapture = theBoard[chessGame.squareToIndex("" + aToH[(aToH.IndexOf(getSquare()[0]) + 1)] + (int.Parse("" + getSquare()[1]) + x))];
                    if ((optionalCapture != null) && (optionalCapture.getColor() != isWhite))
                        possibleCaptures += optionalCapture.getSquare();
                    else if (checkingEnPassantOption & optionalCapture == null)
                        possibleCaptures += "" + aToH[aToH.IndexOf(getSquare()[0]) + 1] + (int.Parse("" + getSquare()[1]) + x);
                }
            }
            return possibleCaptures;
        }
    }

    class pawn: piece
    {
        public pawn(bool isWhite, string square) : base(isWhite,square)
        { }

        public override string move(piece[] theBoard)
            // returns the optional squares the piece can move to based on its own location only
        {
            string possibleMoves = "";
            int x = isWhite ? 1 : -1;   // pawn's moving direction depends on its color
            piece frontSquare = theBoard[chessGame.squareToIndex(getSquare()) + x * 8];
            if (frontSquare == null)
            {
                possibleMoves += "" + getSquare()[0] + (int.Parse("" + getSquare()[1]) + x);
                if ((isWhite & getSquare()[1] == '2') | (!isWhite & getSquare()[1] == '7'))
                    possibleMoves += "" + getSquare()[0] + (int.Parse("" + getSquare()[1]) + x + x);
            }
            return possibleMoves;
        }

        public override string ToString()
        {
            return (isWhite ? "W" : "B") +"P";
        }
    }

    class rook: piece
    {
        public bool madeMove = false;


        public rook(bool isWhite, string square) : base(isWhite, square)
        { }

        public override string move(piece[] theBoard)
        {
        return checkPath(theBoard, false, false);
        }

        public override string ToString()
        {
            return (isWhite ? "W" : "B") + "R";
        }
    }

    class bishop : piece
    {
        public bishop(bool isWhite, string square) : base(isWhite, square)
        { }

        public override string move(piece[] theBoard)
        {
            return checkPath(theBoard, true, false);
        }

        public override string ToString()
        {
            return (isWhite ? "W" : "B") + "B";
        }
    }

    class queen : piece
    {
        public queen(bool isWhite, string square) : base(isWhite, square)
        { }

        public override string move(piece[] theBoard)
        {
            string result = "";
            result += checkPath(theBoard, false, false);
            result += checkPath(theBoard, true, false);
            return result;
        }

        public override string ToString()
        {
            return (isWhite ? "W" : "B") + "Q";
        }
    }

    class king:piece
    {

        public bool madeMove = false;

        public king(bool isWhite, string square) : base(isWhite, square)
        { }

        public override string move(piece[] theBoard)
        {
            string result = "";
            result += checkPath(theBoard, false, true);
            result += checkPath(theBoard, true, true);
            char line = isWhite ? '1' : '8';
            if (!madeMove)
            {
                //small castling
                if ((theBoard[chessGame.squareToIndex("F" + line)] == null) & (theBoard[chessGame.squareToIndex("G" + line)] == null)
                    & (theBoard[chessGame.squareToIndex("H" + line)] != null)
                    && !((rook)theBoard[chessGame.squareToIndex("H" + line)]).madeMove)
                        result += "G" + line;
                //big castling
                if ((theBoard[chessGame.squareToIndex("D" + line)] == null) & (theBoard[chessGame.squareToIndex("C" + line)] == null) &
                    (theBoard[chessGame.squareToIndex("B" + line)] == null) & (theBoard[chessGame.squareToIndex("A" + line)] != null) &&
                    (!((rook)theBoard[chessGame.squareToIndex("A" + line)]).madeMove))
                    result += "C" + line;
            }
            return result;
        }

        public override string ToString()
        {
            return (isWhite ? "W" : "B") + "K";
        }
    }

    class knight : piece
    {
        public knight(bool isWhite, string square) : base(isWhite, square)
        { }

        public override string move(piece[] theBoard)
        {
            return knightMoves();
        }

        public override string ToString()
        {
            return (isWhite ? "W" : "B") + "N";
        }
    }

}
