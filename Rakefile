require 'albacore'
require 'jeweler'
require 'fileutils'

task :default => [:msbuild]

desc 'Compile'
msbuild do |msb|
  msb.properties = {'Configuration' => 'Release'}
  msb.solution = 'src/StructureMap.AutoNotify.sln'
end

build_output = 'src/StructureMap.AutoNotify/bin/Release/StructureMap.AutoNotify.dll'
gem_dll = 'lib/StructureMap.AutoNotify.dll'

task :set_up_lib => gem_dll

file gem_dll => ['lib', build_output] do
  FileUtils.cp build_output, gem_dll
end

directory 'lib'

file build_output => :msbuild

namespace :gem do
  Jeweler::Tasks.new do |gemspec|
    gemspec.name = "structuremap.autonotify"
    gemspec.summary = "A project to automatically implement INotifyPropertyChanged using DynamicProxy2"
    gemspec.description = "A project to automatically implement INotifyPropertyChanged using DynamicProxy2"
    gemspec.email = ["maburke@sep.com", "jcfuller@sep.com"]
    gemspec.authors = ["Jon Fuller", "Matt Burke"]
    gemspec.files = [gem_dll, 'VERSION']
    gemspec.add_dependency 'structuremap'
    gemspec.add_dependency 'castle.dynamicproxy2'
  end
end

%W( gem:gemspec:generate gem:build ).each { |t| task t => :set_up_lib }
