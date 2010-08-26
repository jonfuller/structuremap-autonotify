require 'rubygems'
require 'bundler'
Bundler.setup :default, :rake
require 'albacore'
require 'jeweler'
require 'fileutils'
require 'noodle'

version_file = 'VERSION'
task :default => [:assemblyinfo, :msbuild]

desc 'Compile'
msbuild do |msb|
  msb.properties = {'Configuration' => 'Release'}
  msb.solution = 'src/StructureMap.AutoNotify.sln'
end

file version_file do
  File.open(version_file, 'w') {|f| f.write("0.0.0.0") }
end

assemblyinfo :assemblyinfo => version_file do |asm|
  version = File.open(version_file).read.chomp
  asm.version = version
  asm.file_version = version
  asm.title = 'structuremap-autonotify'
  asm.description = 'A library for dynamically implementing INotifyPropertyChanged'
  asm.output_file = 'src/CommonAssemblyInfo.cs'
end

Noodle::Rake::NoodleTask.new do |noodle|
  noodle.groups << :dotnet
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
    gemspec.email = ["maburke@sep.com", "fullerjc@gmail.com"]
    gemspec.authors = ["Jon Fuller", "Matt Burke"]
    gemspec.files = [gem_dll, version_file]
    gemspec.add_dependency 'structuremap'
    gemspec.add_dependency 'castle.dynamicproxy2'
    gemspec.add_dependency 'log4net'
  end
  Jeweler::GemcutterTasks.new
end

%W( gem:gemspec:generate gem:build ).each { |t| task t => :set_up_lib }
