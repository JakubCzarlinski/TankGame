Module Sort
    Public Function MergeSort(ByVal listToSort As List(Of Single)) As List(Of Single)
        'Complete a merge sort on a list of real numbers
        Dim len As Integer = listToSort.Count()
        Dim lenL As Integer = len \ 2
        Dim lenR As Integer = len - lenL
        Dim leftList As List(Of Single)
        Dim rightList As List(Of Single)
        If len > 1 Then
            ' - divide - split the left and right sides of the lists
            'recurison
            leftList = MergeSort(listToSort.GetRange(0, lenL))
            rightList = MergeSort(listToSort.GetRange(lenL, lenR))

            Dim counterL As Integer = 0
            Dim counterR As Integer = 0

            listToSort.Clear()
            ' - conquer - add the smallest value from each list until the end of any
            'list has been reached. Copy the remaining values of the other list.
            While counterL <> lenL And counterR <> lenR
                If leftList(counterL) < rightList(counterR) Then
                    listToSort.Add(leftList(counterL))
                    counterL += 1
                Else
                    listToSort.Add(rightList(counterR))
                    counterR += 1
                End If
            End While
            If counterL <> lenL Then
                listToSort.AddRange(leftList.GetRange(counterL, lenL - counterL))
            Else
                listToSort.AddRange(rightList.GetRange(counterR, lenR - counterR))
            End If
        End If
        Return listToSort
    End Function
End Module
