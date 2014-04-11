// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
using DragonBones.Objects;
using DragonBones.Utils;
using Com.Viperstudio.Geom;
using Com.Viperstudio.Utils;
using System.Collections.Generic;
namespace DragonBones.Animation
{
	public class TimelineState
	{
		private const float HALF_PI = (float)Math.PI * 0.5f;
		private const float DOUBLE_PI = (float)Math.PI * 2f;
		
		private static List<TimelineState> _pool = new List<TimelineState>();
		
		/** @private */
		public static TimelineState borrowObject()
		{
			if(_pool.Count == 0)
			{
				return new TimelineState();
			}
			TimelineState timelineState = _pool[_pool.Count-1];
			_pool.RemoveAt(_pool.Count-1);
			return timelineState;
		}
		
		/** @private */
		public static void returnObject(TimelineState timeline)
		{
			if(_pool.IndexOf(timeline) < 0)
			{
				_pool[_pool.Count] = timeline;
			}
			
			timeline.clearVaribles();
		}
		
		/** @private */
		public static void clear()
		{
			int i = _pool.Count;
			while(i -- >0)
			{
				_pool[i].clearVaribles();
			}
			_pool.Clear ();
		}
		
		public static float GetEaseValue(float value, float easing)
		{
			float valueEase = float.NaN;
			if (easing > 1)
			{
				valueEase = (float)(0.5 * (1 - Math.Cos(value * Math.PI )) - value);
				easing -= 1;
			}
			else if (easing > 0)
			{
				valueEase = (float)Math.Sin(value * HALF_PI) - value;
			}
			else if (easing < 0)
			{
				valueEase = 1 - (float)Math.Cos(value * HALF_PI) - value;
				easing *= -1;
			}
			return valueEase * easing + value;
		}
		
		public DBTransform Transform;
		public Point Pivot;
		public bool TweenActive;
		
		private int _updateState;
		
		private AnimationState _animationState;
		private Bone _bone;
		private TransformTimeline _timeline;
		private TransformFrame _currentFrame;
		private float _currentFramePosition;
		private float _currentFrameDuration;
		private DBTransform _durationTransform;
		private Point _durationPivot;
		private ColorTransform _durationColor;
		private DBTransform _originTransform;
		private Point _originPivot;
		
		private float _tweenEasing;
		private bool _tweenTransform;
		private bool _tweenColor;
		
		private float _totalTime;
		
		public TimelineState()
		{
			Transform = new DBTransform();
			Pivot = new Point();
			
			//_originTransform = new DBTransform();
			
			_durationTransform = new DBTransform();
			_durationPivot = new Point();
			_durationColor = new ColorTransform();
		}
		
		public void FadeIn(Bone bone, AnimationState animationState, TransformTimeline timeline)
		{
			_bone = bone;
			_animationState = animationState;
			_timeline = timeline;
			
			_originTransform = _timeline.OriginTransform;
			_originPivot = _timeline.OriginPivot;
			//_originTransform.copy(_timeline.originTransform);
			
			/*
			var bLRX:Number = _bone.origin.skewX + _bone.offset.skewX + _bone._tween.skewX;
			var bLRY:Number = _bone.origin.skewY + _bone.offset.skewY + _bone._tween.skewY;
			
			_originTransform.skewX = bLRX + TransformUtils.formatRadian(_originTransform.skewX - bLRX);
			_originTransform.skewY = bLRY + TransformUtils.formatRadian(_originTransform.skewY - bLRY);
			*/
			
			_tweenTransform = false;
			_tweenColor = false;
			
			_totalTime = _animationState.TotalTime;
			
			Transform.X = 0;
			Transform.Y = 0;
			Transform.ScaleX = 0;
			Transform.ScaleY = 0;
			Transform.SkewX = 0;
			Transform.SkewY = 0;
			Pivot.X = 0;
			Pivot.Y = 0;
			
			_durationTransform.X = 0;
			_durationTransform.Y = 0;
			_durationTransform.ScaleX = 0;
			_durationTransform.ScaleY = 0;
			_durationTransform.SkewX = 0;
			_durationTransform.SkewY = 0;
			_durationPivot.X = 0;
			_durationPivot.Y = 0;
			
			_currentFrame = null;
			
			switch(_timeline.FrameList.Count)
			{
			case 0:
				_bone.arriveAtFrame(null, this, _animationState, false);
				_updateState = 0;
				break;
			case 1:
				_updateState = -1;
				break;
			default:
				_updateState = 1;
				break;
			}
		}
		
		public void FadeOut()
		{
			Transform.SkewX = TransformUtil.FormatRadian(Transform.SkewX);
			Transform.SkewY = TransformUtil.FormatRadian(Transform.SkewY);
			//_originTransform.skewX = TransformUtil.formatRadian(_originTransform.skewX);
			//_originTransform.skewY = TransformUtil.formatRadian(_originTransform.skewY);
		}
		
		public void Update(float progress)
		{


			if(_updateState!=0)
			{
				if(_updateState > 0)
				{
					if(_timeline.Scale == 0f)
					{
						progress = 1f;
					}
					else
					{
						progress /= _timeline.Scale;
					}
					
					if(progress == 1f)
					{
						progress = 0.99999999f;
					}
					
					progress += _timeline.Offset;
					int loopCount = (int)progress;
					progress -= loopCount;
					
					//
					float playedTime = _totalTime * progress;
					bool isArrivedFrame = false;
					int frameIndex = 0;

					while (_currentFrame==null || playedTime > _currentFramePosition + _currentFrameDuration || playedTime < _currentFramePosition)
					{
						//Logger.Log ("updating " + playedTime);
						if(isArrivedFrame)
						{
							_bone.arriveAtFrame(_currentFrame, this, _animationState, true);
						}
						isArrivedFrame = true;
						if(_currentFrame!=null)
						{
							//Logger.Log ("updating " + _timeline.Duration  +"  " + _currentFrame.Position + " " + _currentFrame.Duration);
							frameIndex = _timeline.FrameList.IndexOf(_currentFrame) + 1;
							if(frameIndex >= _timeline.FrameList.Count)
							{
								frameIndex = 0;


							}
							_currentFrame = _timeline.FrameList[frameIndex] as TransformFrame;
						}
						else
						{
							frameIndex = 0;
							_currentFrame = _timeline.FrameList[0] as TransformFrame;

						}
						_currentFrameDuration = _currentFrame.Duration;
						_currentFramePosition = _currentFrame.Position;
						//Logger.Log(playedTime + " " + _currentFramePosition + "  " + ( _currentFrameDuration  + _currentFramePosition));
					}

					if(isArrivedFrame)
					{
						TweenActive = _currentFrame.DisplayIndex >= 0;
						frameIndex ++;
						if(frameIndex >= _timeline.FrameList.Count)
						{
							frameIndex = 0;
						}
						TransformFrame nextFrame = _timeline.FrameList[frameIndex] as TransformFrame;
						
						if(
							frameIndex == 0 && 
							_animationState.Loop!=0 && 
							_animationState.LoopCount >= Math.Abs(_animationState.Loop) - 1 && 
							((_currentFramePosition + _currentFrameDuration) / _totalTime + loopCount - _timeline.Offset) * _timeline.Scale > 0.99999999
							)
						{
							_updateState = 0;
							_tweenEasing = float.NaN;
						}
						else if(_currentFrame.DisplayIndex < 0 || nextFrame.DisplayIndex < 0 || !_animationState.TweenEnabled)
						{
							_tweenEasing = float.NaN;
						}
						else if(float.IsNaN(_animationState.Clip.TweenEasing))
						{
							_tweenEasing = _currentFrame.TweenEasing;
						}
						else
						{
							_tweenEasing = _animationState.Clip.TweenEasing;
						}
						
						if(float.IsNaN(_tweenEasing))
						{
							_tweenTransform = false;
							_tweenColor = false;
						}
						else
						{
							_durationTransform.X = nextFrame.Transform.X - _currentFrame.Transform.X;
							_durationTransform.Y = nextFrame.Transform.Y - _currentFrame.Transform.Y;
							_durationTransform.SkewX = nextFrame.Transform.SkewX - _currentFrame.Transform.SkewX;
							_durationTransform.SkewY = nextFrame.Transform.SkewY - _currentFrame.Transform.SkewY;
							_durationTransform.ScaleX = nextFrame.Transform.ScaleX - _currentFrame.Transform.ScaleX;
							_durationTransform.ScaleY = nextFrame.Transform.ScaleY - _currentFrame.Transform.ScaleY;
							
							if(frameIndex == 0)
							{
								_durationTransform.SkewX = TransformUtil.FormatRadian(_durationTransform.SkewX);
								_durationTransform.SkewY = TransformUtil.FormatRadian(_durationTransform.SkewY);
							}
							
							_durationPivot.X = nextFrame.Pivot.X - _currentFrame.Pivot.X;
							_durationPivot.Y = nextFrame.Pivot.Y - _currentFrame.Pivot.Y;
							
							if(
								_durationTransform.X != 0 ||
								_durationTransform.Y != 0 ||
								_durationTransform.SkewX != 0 ||
								_durationTransform.SkewY != 0 ||
								_durationTransform.ScaleX != 0 ||
								_durationTransform.ScaleY != 0 ||
								_durationPivot.X != 0 ||
								_durationPivot.Y != 0
								)
							{
								_tweenTransform = true;
							}
							else
							{
								_tweenTransform = false;
							}
							
							if(_currentFrame.Color!=null && nextFrame.Color!=null)
							{
								_durationColor.AlphaOffset = nextFrame.Color.AlphaOffset - _currentFrame.Color.AlphaOffset;
								_durationColor.RedOffset = nextFrame.Color.RedOffset - _currentFrame.Color.RedOffset;
								_durationColor.GreenOffset = nextFrame.Color.GreenOffset - _currentFrame.Color.GreenOffset;
								_durationColor.BlueOffset = nextFrame.Color.BlueOffset - _currentFrame.Color.BlueOffset;
								
								_durationColor.AlphaMultiplier = nextFrame.Color.AlphaMultiplier - _currentFrame.Color.AlphaMultiplier;
								_durationColor.RedMultiplier = nextFrame.Color.RedMultiplier - _currentFrame.Color.RedMultiplier;
								_durationColor.GreenMultiplier = nextFrame.Color.GreenMultiplier - _currentFrame.Color.GreenMultiplier;
								_durationColor.BlueMultiplier = nextFrame.Color.BlueMultiplier - _currentFrame.Color.BlueMultiplier;
								
								if(
									_durationColor.AlphaOffset != 0 ||
									_durationColor.RedOffset != 0 ||
									_durationColor.GreenOffset != 0 ||
									_durationColor.BlueOffset != 0 ||
									_durationColor.AlphaMultiplier != 0 ||
									_durationColor.RedMultiplier != 0 ||
									_durationColor.GreenMultiplier != 0 ||
									_durationColor.BlueMultiplier != 0 
									)
								{
									_tweenColor = true;
								}
								else
								{
									_tweenColor = false;
								}
							}
							else if(_currentFrame.Color!=null)
							{
								_tweenColor = true;
								_durationColor.AlphaOffset = -_currentFrame.Color.AlphaOffset;
								_durationColor.RedOffset = -_currentFrame.Color.RedOffset;
								_durationColor.GreenOffset = -_currentFrame.Color.GreenOffset;
								_durationColor.BlueOffset = -_currentFrame.Color.BlueOffset;
								
								_durationColor.AlphaMultiplier = 1 - _currentFrame.Color.AlphaMultiplier;
								_durationColor.RedMultiplier = 1 - _currentFrame.Color.RedMultiplier;
								_durationColor.GreenMultiplier = 1 - _currentFrame.Color.GreenMultiplier;
								_durationColor.BlueMultiplier = 1 - _currentFrame.Color.BlueMultiplier;
							}
							else if(nextFrame.Color!=null)
							{
								_tweenColor = true;
								_durationColor.AlphaOffset = nextFrame.Color.AlphaOffset;
								_durationColor.RedOffset = nextFrame.Color.RedOffset;
								_durationColor.GreenOffset = nextFrame.Color.GreenOffset;
								_durationColor.BlueOffset = nextFrame.Color.BlueOffset;
								
								_durationColor.AlphaMultiplier = nextFrame.Color.AlphaMultiplier - 1;
								_durationColor.RedMultiplier = nextFrame.Color.RedMultiplier - 1;
								_durationColor.GreenMultiplier = nextFrame.Color.GreenMultiplier - 1;
								_durationColor.BlueMultiplier = nextFrame.Color.BlueMultiplier - 1;
							}
							else
							{
								_tweenColor = false;
							}
						}
						
						if(!_tweenTransform)
						{
							if(_animationState.Blend)
							{
								Transform.X = _originTransform.X + _currentFrame.Transform.X;
								Transform.Y = _originTransform.Y + _currentFrame.Transform.Y;
								Transform.SkewX = _originTransform.SkewX + _currentFrame.Transform.SkewX;
								Transform.SkewY = _originTransform.SkewY + _currentFrame.Transform.SkewY;
								Transform.ScaleX = _originTransform.ScaleX + _currentFrame.Transform.ScaleX;
								Transform.ScaleY = _originTransform.ScaleY + _currentFrame.Transform.ScaleY;
								
								Pivot.X = _originPivot.X + _currentFrame.Pivot.X;
								Pivot.Y = _originPivot.Y + _currentFrame.Pivot.Y;
							}
							else
							{
								Transform.X = _currentFrame.Transform.X;
								Transform.Y = _currentFrame.Transform.Y;
								Transform.SkewX = _currentFrame.Transform.SkewX;
								Transform.SkewY = _currentFrame.Transform.SkewY;
								Transform.ScaleX = _currentFrame.Transform.ScaleX;
								Transform.ScaleY = _currentFrame.Transform.ScaleY;
								
								Pivot.X = _currentFrame.Pivot.X;
								Pivot.Y = _currentFrame.Pivot.Y;
							}
						}
						
						if(!_tweenColor)
						{
							if(_currentFrame.Color!=null)
							{
								_bone.updateColor(
									_currentFrame.Color.AlphaOffset, 
									_currentFrame.Color.RedOffset, 
									_currentFrame.Color.GreenOffset, 
									_currentFrame.Color.BlueOffset, 
									_currentFrame.Color.AlphaMultiplier, 
									_currentFrame.Color.RedMultiplier, 
									_currentFrame.Color.GreenMultiplier, 
									_currentFrame.Color.BlueMultiplier, 
									true
									);
							}
							else if(_bone._isColorChanged)
							{
								_bone.updateColor(0, 0, 0, 0, 1, 1, 1, 1, false);
							}
						}
						_bone.arriveAtFrame(_currentFrame, this, _animationState, false);
					}
					
					if(_tweenTransform || _tweenColor)
					{
						progress = (playedTime - _currentFramePosition) / _currentFrameDuration;
						if(_tweenEasing!=float.NaN && _tweenEasing!=0)
						{
							progress = GetEaseValue(progress, _tweenEasing);
						}
					}
					
					if(_tweenTransform)
					{
						DBTransform currentTransform = _currentFrame.Transform;
						Point currentPivot = _currentFrame.Pivot;
						if(_animationState.Blend)
						{
							Transform.X = _originTransform.X + currentTransform.X + _durationTransform.X * progress;
							Transform.Y = _originTransform.Y + currentTransform.Y + _durationTransform.Y * progress;
							Transform.SkewX = _originTransform.SkewX + currentTransform.SkewX + _durationTransform.SkewX * progress;
							Transform.SkewY = _originTransform.SkewY + currentTransform.SkewY + _durationTransform.SkewY * progress;
							Transform.ScaleX = _originTransform.ScaleX + currentTransform.ScaleX + _durationTransform.ScaleX * progress;
							Transform.ScaleY = _originTransform.ScaleY + currentTransform.ScaleY + _durationTransform.ScaleY * progress;
							
							Pivot.X = _originPivot.X + currentPivot.X + _durationPivot.X * progress;
							Pivot.Y = _originPivot.Y + currentPivot.Y + _durationPivot.Y * progress;
						}
						else
						{
							Transform.X = currentTransform.X + _durationTransform.X * progress;
							Transform.Y = currentTransform.Y + _durationTransform.Y * progress;
							Transform.SkewX = currentTransform.SkewX + _durationTransform.SkewX * progress;
							Transform.SkewY = currentTransform.SkewY + _durationTransform.SkewY * progress;
							Transform.ScaleX = currentTransform.ScaleX + _durationTransform.ScaleX * progress;
							Transform.ScaleY = currentTransform.ScaleY + _durationTransform.ScaleY * progress;
							
							Pivot.X = currentPivot.X + _durationPivot.X * progress;
							Pivot.Y = currentPivot.Y + _durationPivot.Y * progress;
						}
					}
					
					if(_tweenColor)
					{
						if(_currentFrame.Color!=null)
						{
							_bone.updateColor(
								_currentFrame.Color.AlphaOffset + _durationColor.AlphaOffset * progress,
								_currentFrame.Color.RedOffset + _durationColor.RedOffset * progress,
								_currentFrame.Color.GreenOffset + _durationColor.GreenOffset * progress,
								_currentFrame.Color.BlueOffset + _durationColor.BlueOffset * progress,
								_currentFrame.Color.AlphaMultiplier + _durationColor.AlphaMultiplier * progress,
								_currentFrame.Color.RedMultiplier + _durationColor.RedMultiplier * progress,
								_currentFrame.Color.GreenMultiplier + _durationColor.GreenMultiplier * progress,
								_currentFrame.Color.BlueMultiplier + _durationColor.BlueMultiplier * progress,
								true
								);
						}
						else
						{
							_bone.updateColor(
								_durationColor.AlphaOffset * progress,
								_durationColor.RedOffset * progress,
								_durationColor.GreenOffset * progress,
								_durationColor.BlueOffset * progress,
								1 + _durationColor.AlphaMultiplier * progress,
								1 + _durationColor.RedMultiplier * progress,
								1 + _durationColor.GreenMultiplier * progress,
								1 + _durationColor.BlueMultiplier * progress,
								true
								);
						}
					}
				}
				else
				{
					_updateState = 0;
					if(_animationState.Blend)
					{
						Transform.Copy(_originTransform);
						
						Pivot.X = _originPivot.X;
						Pivot.Y = _originPivot.Y;
					}
					else
					{
						Transform.X = 
							Transform.Y = 
								Transform.SkewX = 
								Transform.SkewY = 
								Transform.ScaleX = 
								Transform.ScaleY = 0;
						
						Pivot.X = 0;
						Pivot.Y = 0;
					}
					
					_currentFrame = _timeline.FrameList[0] as TransformFrame;
					
					TweenActive = _currentFrame.DisplayIndex >= 0;
					
					if(_currentFrame.Color!=null)
					{
						_bone.updateColor(
							_currentFrame.Color.AlphaOffset, 
							_currentFrame.Color.RedOffset, 
							_currentFrame.Color.GreenOffset, 
							_currentFrame.Color.BlueOffset, 
							_currentFrame.Color.AlphaMultiplier, 
							_currentFrame.Color.RedMultiplier, 
							_currentFrame.Color.GreenMultiplier, 
							_currentFrame.Color.BlueMultiplier,
							true
							);
					}
					else
					{
						_bone.updateColor(0, 0, 0, 0, 1, 1, 1, 1, false);
					}
					
					
					_bone.arriveAtFrame(_currentFrame, this, _animationState, false);
				}
			}
		}
		
		private void clearVaribles()
		{
			_updateState = 0;
			_bone = null;
			_animationState = null;
			_timeline = null;
			_currentFrame = null;
			_originTransform = null;
			_originPivot = null;
		}
	}
}
