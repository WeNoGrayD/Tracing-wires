   M S B A M L   `   `   `  ����P  LTraceWiresApplication, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:TraceWiresApplicationTraceWiresApplication  5   	   N JTraceWiresViewModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullV>clr-namespace:TraceWiresViewModel;assembly=TraceWiresViewModelTraceWiresViewModel 5   	   M ISystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089/$clr-namespace:System;assembly=SystemSystem 5   	   &     TraceWiresApplication.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:TraceWiresApplication     5   	   Mvm>clr-namespace:TraceWiresViewModel;assembly=TraceWiresViewModel     5   	   4sys$clr-namespace:System;assembly=System     5   	     � Title$  
MainWindow��5
   	   $	��450��6   $	��800��6)    � 	Resources 5       �� 
TargetType �� Property �� Value   grCellIcons6  0TraceWiresViewModel.StringToImageSourceConverter  Key 
 Shared ��Name K� ZIndex��%  '��       &	  p    �� 5   
   " ��6   .�5      ��  � ��$
 50px��6&   �� 5       � ��$
 50px��6'   5      � 5   
    5      ��  5      cellImgSrcConverter true 5   2   5      .+�5      �� $	��1.5q�6   ���#6800EF5      �� 5      $ imgCellContent��6   $
��30px��5      $
��30px��6!   $ 0��6/   $
��Fill=�5      �� 5!      $ imgCellPriority��6   $
��30px��5"      $
��30px��6   $ 1��6+   $
��Fill=�5#      5%      5&      .��5(      � -   5         �Name$ grDWSScreenContainer��5(       5*   
   3  -TraceWiresViewModel.DWSScrollAbilityConverter  scrollAbilityConverter��%�   '��       &	 p   �� 5,      " ��6   .�5-      ��  � ��$
 40px��6*   �� 5.       � ��$
 40px��6+   5/        51      52   +   54       � RowDefinitions 56   
   �� 57      $��1* �6   �� 58      $��5* �6   �� 59      $��1* �6   �� 5:      $��1* �6   	 � ColumnDefinitions	 5=   
   �� 5>      $��1* �6   �� 5?      $��2* �6   �� 5@      $��1* �6   .+�5C   
   �� -   5        
 ��Name$
 	btnUpLeft��5C      $��0��5E      $��0��6   {�5F      �� *
CPointBias+ �� 	Converter# [  �� ConverterParameter$ UpLeft��.��5I      �� ��5J      �� 5K       �� Triggers 5L      �� 5M       �� Binding 6*   ��  �� ElementName$ 	btnUpLeft�� �� Path$ 	IsEnabled� �� Value	 false5O   "   .a�5P   "   ��  � ��$$ icons\ScrollUpLeftDisabled.png��5Q   !   5R      �� 5S       6*   �� $ 	btnUpLeft��$ 	IsEnabled� true5U   "   .a�5V   "   ��  � ��$# icons\ScrollUpLeftEnabled.png��5W   !   5X      5Z      5\      5]      �� 5^   
   -   5        $
 btnUp��5^      $��0��5`      $��1��6   {�5a      �� *
CPointBias+# [ $ Up��.��5d      �� ��5e      �� 5f       5g      �� 5h       6*   �� $ btnUp��$ 	IsEnabled�	 false5j   "   .a�5k   "   ��  � ��$  icons\ScrollUpDisabled.png��5l   !   5m      �� 5n       6*   �� $ btnUp��$ 	IsEnabled� true5p   "   .a�5q   "   ��  � ��$ icons\ScrollUpEnabled.png��5r   !   5s      5u      5w      5x      �� 5y   
   -   5        $
 
btnUpRight��5y      $��0��5{      $��2��6   {�5|      �� *
CPointBias+# [ $ UpRight��.��5      �� ��5�      �� 5�       5�      �� 5�       6*   �� $ 
btnUpRight��$ 	IsEnabled�	 false5�   "   .a�5�   "   ��  � ��$% icons\ScrollUpRightDisabled.png��5�   !   5�      �� 5�       6*   �� $ 
btnUpRight��$ 	IsEnabled� true5�   "   .a�5�   "   ��  � ��$$ icons\ScrollUpRightEnabled.png��5�   !   5�      5�      5�      5�      �� 5�   
   -   5        $
 btnLeft��5�      $��1��5�      $��0��6   {�5�      �� *
CPointBias+# [ $
 Left��.��5�      �� ��5�      �� 5�       5�      �� 5�       6*   �� $ btnLeft��$ 	IsEnabled�	 false5�   "   .a�5�   "   ��  � ��$" icons\ScrollLeftDisabled.png��5�   !   5�      �� 5�       6*   �� $ btnLeft��$ 	IsEnabled� true5�   "   .a�5�   "   ��  � ��$! icons\ScrollLeftEnabled.png��5�   !   5�      5�      5�      5�      �� 5�   
   -   5        $
 btnRight��5�      $��1��5�      $��2��6   {�5�      �� *
CPointBias+# [ $ Right��.��5�      �� ��5�      �� 5�       5�      �� 5�       6*   �� $ btnRight��$ 	IsEnabled�	 false5�   "   .a�5�   "   ��  � ��$# icons\ScrollRightDisabled.png��5�   !   5�      �� 5�       6*   �� $ btnRight��$ 	IsEnabled� true5�   "   .a�5�   "   ��  � ��$" icons\ScrollRightEnabled.png��5�   !   5�      5�      5�      5�      �� 5�   
   -   5        $
 btnDownLeft��5�      $��2��5�      $��0��6   {�5�      �� *
CPointBias+# [ $ DownLeft��.��5�      �� ��5�      �� 5�       5�      �� 5�       6*   �� $ btnDownLeft��$ 	IsEnabled�	 false5�   "   .a�5�   "   ��  � ��$&  icons\ScrollDownLeftDisabled.png��5�   !   5�      �� 5�       6*   �� $ btnDownLeft��$ 	IsEnabled� true5�   "   .a�5�   "   ��  � ��$% icons\ScrollDownLeftEnabled.png��5�   !   5�      5�      5�      5�      �� 5�   
   -   5        $
 btnDown��5�      $��2��5�      $��1��6   {�5�      �� *
CPointBias+# [ $
 Down��.��5�      �� ��5�      �� 5�       5�      �� 5�       6*   �� $ btnDown��$ 	IsEnabled�	 false5�   "   .a�5�   "   ��  � ��$" icons\ScrollDownDisabled.png��5�   !   5�      �� 5�       6*   �� $ btnDown��$ 	IsEnabled� true5�   "   .a�5�   "   ��  � ��$! icons\ScrollDownEnabled.png��5�   !   5�      5�      5      5     �� 5  
   -	   5        $
 btnDownRight��5     $��2��5     $��2��6   {�5     �� *
CPointBias+# [ $ 	DownRight��.��5     �� ��5	     �� 5
      5     �� 5      6*   �� $ btnDownRight��$ 	IsEnabled�	 false5  "   .a�5  "   ��  � ��$' !icons\ScrollDownRightDisabled.png��5  !   5     �� 5      6*   �� $ btnDownRight��$ 	IsEnabled� true5  "   .a�5  "   ��  � ��$&  icons\ScrollDownRightEnabled.png��5  !   5     5     5     5     � 5  
   -
   5        $ grDWSScreen��5     $��1��5     $��1��6   
���   �.+�5!     �� $	��1.5q�6   ���#6800EF5"     5#     �� 5%  
   -   5        $��Trace By step��5%     $��20(�6)   $	��180��67   $��3��6C   $��0��6P   5&     �� 5(  
   -   5        $��
Trace Auto��5(     $��20(�6&   $	��200��64   $��3��6@   $��1��6M   5)     �� 5+  
   -   5        $��Cancel tracing��5+     $��20(�6*   $	��200��68   $��3��6D   $��2��6Q   5,     5.     5/     