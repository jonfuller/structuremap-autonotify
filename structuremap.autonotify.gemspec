# Generated by jeweler
# DO NOT EDIT THIS FILE DIRECTLY
# Instead, edit Jeweler::Tasks in rakefile, and run the gemspec command
# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{structuremap.autonotify}
  s.version = "0.2.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Jon Fuller", "Matt Burke"]
  s.date = %q{2010-08-26}
  s.description = %q{A project to automatically implement INotifyPropertyChanged using DynamicProxy2}
  s.email = ["maburke@sep.com", "fullerjc@gmail.com"]
  s.files = [
    "VERSION",
     "lib/StructureMap.AutoNotify.dll"
  ]
  s.rdoc_options = ["--charset=UTF-8"]
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.7}
  s.summary = %q{A project to automatically implement INotifyPropertyChanged using DynamicProxy2}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::VERSION) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<structuremap>, [">= 0"])
      s.add_runtime_dependency(%q<castle.dynamicproxy2>, [">= 0"])
      s.add_runtime_dependency(%q<log4net>, [">= 0"])
    else
      s.add_dependency(%q<structuremap>, [">= 0"])
      s.add_dependency(%q<castle.dynamicproxy2>, [">= 0"])
      s.add_dependency(%q<log4net>, [">= 0"])
    end
  else
    s.add_dependency(%q<structuremap>, [">= 0"])
    s.add_dependency(%q<castle.dynamicproxy2>, [">= 0"])
    s.add_dependency(%q<log4net>, [">= 0"])
  end
end

